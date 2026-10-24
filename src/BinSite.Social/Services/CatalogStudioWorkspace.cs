using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Owns catalog validation, image rebinding, atomic bulk changes and portable project serialization.</summary>
public static partial class CatalogStudioWorkspace
{
    public const int MaximumImages = 200;
    public const int MaximumProjectCharacters = 45_000_000;
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true, MaxDepth = 32 };

    /// <summary>Reads a nonnegative ungrouped decimal using the explicitly chosen separator.</summary>
    public static bool TryPrice(string? value, string separator, out decimal price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 50 || separator is not ("." or ",")) return false;
        var culture = separator == "," ? CultureInfo.GetCultureInfo("fr-FR") : CultureInfo.InvariantCulture;
        return decimal.TryParse(value.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, culture, out price)
            && price is >= 0 and <= 1_000_000_000m;
    }

    /// <summary>Formats a validated amount consistently without implying tax, checkout or currency conversion.</summary>
    public static string PriceLabel(CatalogStudioProduct product, CatalogStudioProject project) =>
        TryPrice(product.Price, project.DecimalSeparator, out var value)
            ? value.ToString("0.00", CultureInfo.InvariantCulture) + " " + project.Currency : "Invalid price";

    /// <summary>Reports all repairable row issues, including excluded rows which do not block export.</summary>
    public static List<CatalogStudioIssue> Validate(CatalogStudioProject project)
    {
        EnsureShape(project, false);
        var issues = new List<CatalogStudioIssue>();
        if (string.IsNullOrWhiteSpace(project.Title)) issues.Add(new(0, "Title", "Enter a catalog title.", true));
        if (!CurrencyPattern().IsMatch(project.Currency)) issues.Add(new(0, "Currency", "Use a three-letter uppercase currency code.", true));
        var duplicates = project.Products.Where(product => product.Included && product.Sku.Trim().Length > 0)
            .GroupBy(product => product.Sku.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var images = project.Images.ToDictionary(image => image.Id, StringComparer.Ordinal);
        foreach (var product in project.Products)
        {
            void Add(string field, string message, bool error = true) => issues.Add(new(product.SourceRow, field, message, error, product.Id));
            if (string.IsNullOrWhiteSpace(product.Sku)) Add("SKU", "SKU is missing.");
            else if (product.Sku.Length > 80) Add("SKU", "SKU exceeds 80 characters.");
            else if (duplicates.TryGetValue(product.Sku.Trim(), out var repeated))
                Add("SKU", $"Duplicate SKU is used by {repeated} included rows. Repair the SKU or exclude the extra rows.");
            if (string.IsNullOrWhiteSpace(product.Name)) Add("Name", "Product name is missing.");
            else if (product.Name.Length > 160) Add("Name", "Name exceeds 160 characters.");
            if (!TryPrice(product.Price, project.DecimalSeparator, out _)) Add("Price", "Enter a nonnegative ungrouped price using the selected decimal separator.");
            if (product.Description.Length > 4000) Add("Description", "Description exceeds 4,000 characters.");
            if (product.Category.Length > 100) Add("Category", "Category exceeds 100 characters.");
            if (product.Stock.Length > 0 && (!int.TryParse(product.Stock, NumberStyles.None, CultureInfo.InvariantCulture, out var stock) || stock > 100_000_000))
                Add("Stock", "Stock must be blank or a nonnegative whole number up to 100,000,000.");
            if (product.ImageHint.Length > 200) Add("Image", "Image filename exceeds 200 characters.");
            var image = product.ImageId is not null && images.TryGetValue(product.ImageId, out var attached) ? attached : null;
            if (image is null) Add("Image", "No image is matched. Attach a photo or choose one manually.", false);
            else if (!image.Attached) Add("Image", "Only the image reference is present. Reattach the local photo to include it in export.", false);
        }
        return issues;
    }

    /// <summary>Checks bounded schema and allowed image payloads while allowing repairable product-value errors.</summary>
    public static void EnsureShape(CatalogStudioProject project, bool validateImagePayloads = true)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (project.FormatVersion != 1 || project.Title is null || project.Title.Length > 160 || project.Description is null || project.Description.Length > 4000
            || project.Currency is null || project.Currency.Length > 8 || project.DecimalSeparator is not ("." or ",")
            || project.Products is null || project.Products.Count > CatalogStudioCsv.MaximumRows || project.Images is null || project.Images.Count > MaximumImages)
            throw new FormatException("This project has an unsupported format or exceeds the workspace limits.");
        var productIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var product in project.Products)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.Id) || product.Id.Length > 80 || !productIds.Add(product.Id) || product.SourceRow < 0
                || new[] { product.Sku, product.Name, product.Price, product.Stock, product.Description, product.Category, product.ImageHint }.Any(value => value is null || value.Length > 5000)
                || product.ImageId?.Length > 80)
                throw new FormatException("The project contains invalid or duplicate product identifiers or oversized fields.");
        }
        var imageIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var image in project.Images)
        {
            if (image is null || string.IsNullOrWhiteSpace(image.Id) || image.Id.Length > 80 || !imageIds.Add(image.Id)
                || image.Name is null || image.Name.Length > 200 || image.Fingerprint is null || image.Fingerprint.Length > 200
                || image.Width < 0 || image.Height < 0 || image.Width > 2400 || image.Height > 2400 || image.DataUrl is null || image.DataUrl.Length > 2_800_000)
                throw new FormatException("An image reference or image size is invalid.");
            if (validateImagePayloads && image.Attached && !IsAllowedImage(image.DataUrl)) throw new FormatException("An image payload is unsupported. Use PNG, JPEG, WebP or an original bundled demo SVG.");
        }
    }

    /// <summary>Permits raster data URLs with matching signatures and only exact, application-authored SVG assets.</summary>
    public static bool IsAllowedImage(string dataUrl)
    {
        if (CatalogStudioSamples.IsDemoImage(dataUrl)) return true;
        var comma = dataUrl.IndexOf(',');
        if (comma < 0 || dataUrl.Length > 2_800_000) return false;
        var header = dataUrl[..comma];
        if (header is not ("data:image/png;base64" or "data:image/jpeg;base64" or "data:image/webp;base64")) return false;
        byte[] bytes;
        try { bytes = Convert.FromBase64String(dataUrl[(comma + 1)..]); }
        catch (FormatException) { return false; }
        return header switch
        {
            "data:image/png;base64" => bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "data:image/jpeg;base64" => bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255,
            _ => bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8)
        };
    }

    /// <summary>Reattaches matching fingerprints, then links unassigned rows by exact filename or SKU stem.</summary>
    public static int AttachImages(CatalogStudioProject project, IEnumerable<CatalogStudioImage> incoming)
    {
        var accepted = incoming.ToList();
        EnsureShape(new CatalogStudioProject { Images = accepted });
        var next = project.Images.Select(image => new CatalogStudioImage
        {
            Id = image.Id, Name = image.Name, DataUrl = image.DataUrl, Fingerprint = image.Fingerprint, Width = image.Width, Height = image.Height
        }).ToList();
        foreach (var image in accepted)
        {
            var old = image.Fingerprint.Length > 0 ? next.FirstOrDefault(item => item.Fingerprint == image.Fingerprint) : null;
            if (old is not null)
            {
                old.DataUrl = image.DataUrl;
                old.Width = image.Width;
                old.Height = image.Height;
                old.Name = image.Name;
            }
            else
            {
                if (next.Count >= MaximumImages) throw new FormatException($"The image bank is limited to {MaximumImages} images.");
                next.Add(new()
                {
                    Id = next.Any(item => item.Id == image.Id) ? Guid.NewGuid().ToString("N") : image.Id,
                    Name = image.Name, DataUrl = image.DataUrl, Fingerprint = image.Fingerprint, Width = image.Width, Height = image.Height
                });
            }
        }
        if (next.Sum(image => (long)image.DataUrl.Length) > 40_000_000) throw new FormatException("The attached image bank exceeds 40 MB. Use fewer or smaller photos.");
        project.Images = next;
        return MatchImages(project);
    }

    /// <summary>Matches only unassigned products; ambiguous candidates remain available for manual selection.</summary>
    public static int MatchImages(CatalogStudioProject project)
    {
        var matched = 0;
        foreach (var product in project.Products)
        {
            if (product.ImageId is not null && project.Images.Any(image => image.Id == product.ImageId)) continue;
            var hint = FileName(product.ImageHint);
            var exact = hint.Length > 0 ? project.Images.Where(image => FileName(image.Name).Equals(hint, StringComparison.OrdinalIgnoreCase)).ToArray() : [];
            var candidates = exact.Length > 0 ? exact : project.Images.Where(image => Stem(image.Name).Equals(product.Sku.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
            if (candidates.Length == 1) { product.ImageId = candidates[0].Id; matched++; }
        }
        return matched;
    }

    /// <summary>Produces a portable JSON snapshot; metadata-only mode removes image bytes but retains rebinding references.</summary>
    public static string Serialize(CatalogStudioProject project, bool metadataOnly = false)
    {
        EnsureShape(project);
        if (!metadataOnly)
        {
            var json = JsonSerializer.Serialize(project, JsonOptions);
            if (Encoding.UTF8.GetByteCount(json) > MaximumProjectCharacters) throw new FormatException("Project exceeds the 45 MB portable file limit. Remove unused images before saving.");
            return json;
        }
        var metadata = new CatalogStudioProject
        {
            Title = project.Title, Description = project.Description, Currency = project.Currency, DecimalSeparator = project.DecimalSeparator,
            Products = project.Products,
            Images = project.Images.Select(image => new CatalogStudioImage
            {
                Id = image.Id, Name = image.Name, Fingerprint = image.Fingerprint, Width = image.Width, Height = image.Height
            }).ToList()
        };
        return JsonSerializer.Serialize(metadata, JsonOptions);
    }

    /// <summary>Restores validated project state without mutating the caller's current workspace on failure.</summary>
    public static CatalogStudioProject Deserialize(string json)
    {
        if (json.Length > MaximumProjectCharacters || Encoding.UTF8.GetByteCount(json) > MaximumProjectCharacters) throw new FormatException("Project exceeds the 45 MB file limit.");
        CatalogStudioProject project;
        try { project = JsonSerializer.Deserialize<CatalogStudioProject>(json.TrimStart('\uFEFF'), JsonOptions) ?? throw new FormatException("Project is empty."); }
        catch (JsonException error) { throw new FormatException("The project is not valid catalog JSON.", error); }
        EnsureShape(project);
        return project;
    }

    /// <summary>Applies selected-row changes atomically after validating all proposed numeric values.</summary>
    public static int BulkEdit(CatalogStudioProject project, IReadOnlySet<string> selected, string? category, string? stock, decimal? pricePercent)
    {
        var products = project.Products.Where(product => selected.Contains(product.Id)).ToArray();
        if (products.Length == 0) throw new FormatException("Select at least one product.");
        if (category?.Length > 100) throw new FormatException("Category exceeds 100 characters.");
        if (stock is not null && stock.Length > 0 && (!int.TryParse(stock, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count > 100_000_000))
            throw new FormatException("Bulk stock must be blank or a nonnegative whole number.");
        if (pricePercent is < -100 or > 1000) throw new FormatException("Price adjustment must be from -100% to 1000%.");
        var prices = new Dictionary<string, string>();
        if (pricePercent is not null)
            foreach (var product in products)
            {
                if (!TryPrice(product.Price, project.DecimalSeparator, out var price)) throw new FormatException($"Row {product.SourceRow} has an invalid price. Repair it before adjusting prices.");
                var adjusted = decimal.Round(price * (1 + pricePercent.Value / 100), 2, MidpointRounding.AwayFromZero);
                if (adjusted > 1_000_000_000m) throw new FormatException("A proposed price exceeds the maximum amount.");
                prices[product.Id] = adjusted.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", project.DecimalSeparator);
            }
        foreach (var product in products)
        {
            if (category is not null) product.Category = category.Trim();
            if (stock is not null) product.Stock = stock.Trim();
            if (prices.TryGetValue(product.Id, out var price)) product.Price = price;
        }
        return products.Length;
    }

    /// <summary>Reads a filename from either slash convention without treating it as a path to access.</summary>
    static string FileName(string name) => name.Replace('\\', '/').Split('/').Last().Trim();

    /// <summary>Removes only the final extension for exact SKU-to-filename matching.</summary>
    static string Stem(string name)
    {
        var file = FileName(name);
        var dot = file.LastIndexOf('.');
        return dot > 0 ? file[..dot] : file;
    }

    [GeneratedRegex("^[A-Z]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex CurrencyPattern();
}
