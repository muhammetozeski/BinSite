using System.Globalization;
using System.Net;
using System.Text;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Supplies twenty original fictional products and small assembly-local SVG illustrations.</summary>
public static class CatalogStudioSamples
{
    static readonly (string Name, string Category, decimal Price, int Stock, string Description)[] Items =
    [
        ("Field Notebook", "Stationery", 12.50m, 48, "A compact, thread-bound notebook with dot-grid pages and a durable card cover."),
        ("Pocket Pencil Set", "Stationery", 7.20m, 72, "Three short graphite pencils in a reusable sleeve for notes on the move."),
        ("Clip Reading Lamp", "Desk", 24.00m, 18, "A small adjustable lamp with a broad clip and a focused reading light."),
        ("Canvas Market Tote", "Carry", 19.50m, 36, "A plain canvas tote with a wide opening and reinforced handles."),
        ("Desktop Letter Tray", "Desk", 22.00m, 15, "A low-profile tray for letters, small notebooks and the papers you need today."),
        ("Travel Cup", "Kitchen", 18.75m, 30, "A reusable cup with a fitted lid and an easy-grip band. Sample specification only."),
        ("Trail Water Bottle", "Carry", 26.00m, 27, "A narrow stainless-style bottle with a loop cap for everyday travel."),
        ("Felt Cable Pouch", "Carry", 14.00m, 40, "A soft zip pouch that keeps a charger and small cables together."),
        ("Foldaway Reading Stand", "Desk", 31.50m, 12, "An angled stand with a lower lip for a book or a paper reference sheet."),
        ("Cable Wrap Pair", "Desk", 5.50m, 100, "Two simple reusable straps for keeping coiled cables tidy."),
        ("City Lines Postcards", "Stationery", 9.00m, 60, "Six original line-art postcards with an uncoated writing surface."),
        ("Seed Envelope Pack", "Garden", 6.80m, 55, "Ten paper envelopes with space to label a seed variety and collection date."),
        ("Workshop Ruler", "Stationery", 8.25m, 32, "A flat 20-centimeter-style ruler for sketches and ordinary desk tasks."),
        ("Open Sketch Pad", "Stationery", 16.00m, 25, "A plain drawing pad designed to lie open while planning a small project."),
        ("Window Planter", "Garden", 21.00m, 16, "A small planter with a removable inner cup and a simple water-catching base."),
        ("Woven Coaster Set", "Kitchen", 13.50m, 44, "Four square coasters with a visible woven pattern and stitched borders."),
        ("Page Keeper Clip", "Stationery", 4.75m, 85, "A broad bookmark clip that marks a page without covering the whole line."),
        ("Canvas Tool Roll", "Carry", 29.00m, 20, "A roll-up organizer with five pockets for small hand tools or drawing supplies."),
        ("Pocket Pin Board", "Desk", 17.50m, 22, "A compact framed board for a short checklist, ticket or reminder card."),
        ("Undated Desk Calendar", "Desk", 15.00m, 38, "A reusable planning card stand with undated month sheets for a flexible start.")
    ];
    static readonly string[] ImagePayloads = Enumerable.Range(0, Items.Length).Select(CreateImageData).ToArray();
    static readonly HashSet<string> AllowedDemoImages = new(ImagePayloads, StringComparer.Ordinal);

    /// <summary>Creates an independent editable sample project; all inventory and prices are explicitly fictional.</summary>
    public static CatalogStudioProject Create()
    {
        var project = new CatalogStudioProject
        {
            Title = "Studio Supply — Example Catalog",
            Description = "Twenty original fictional sample products. All prices, specifications and stock counts are demonstration data, not a real offer or live inventory.",
            Currency = "USD"
        };
        for (var index = 0; index < Items.Length; index++)
        {
            var item = Items[index];
            var sku = $"ST-{index + 1:000}";
            var imageId = $"demo-image-{index + 1:000}";
            project.Products.Add(new()
            {
                Id = $"demo-product-{index + 1:000}", SourceRow = index + 2, Sku = sku, Name = item.Name,
                Description = item.Description, Category = item.Category, Price = item.Price.ToString("0.00", CultureInfo.InvariantCulture),
                Stock = item.Stock.ToString(CultureInfo.InvariantCulture), ImageHint = sku + ".svg", ImageId = imageId
            });
            project.Images.Add(new()
            {
                Id = imageId, Name = sku + ".svg", DataUrl = ImagePayloads[index], Width = 400, Height = 260,
                Fingerprint = $"catalog-studio-demo:{sku}:v1"
            });
        }
        return project;
    }

    /// <summary>Exports the sample sheet with quoted fields, including embedded punctuation.</summary>
    public static string Csv()
    {
        static string Quote(string value) => '"' + value.Replace("\"", "\"\"") + '"';
        var project = Create();
        var lines = new List<string> { "SKU,Product Name,Price,Category,Description,Stock,Image Filename" };
        lines.AddRange(project.Products.Select(product => string.Join(',', new[]
        {
            product.Sku, product.Name, product.Price, product.Category, product.Description, product.Stock, product.ImageHint
        }.Select(Quote))));
        return string.Join("\r\n", lines);
    }

    /// <summary>Recognizes only exact embedded SVG bytes, preventing an imported project from supplying active SVG content.</summary>
    public static bool IsDemoImage(string dataUrl) => AllowedDemoImages.Contains(dataUrl);

    /// <summary>Draws a distinct local product illustration without fetching a remote image.</summary>
    static string CreateImageData(int index)
    {
        var colors = new[] { "#165DCD", "#087A45", "#C52B28", "#B75D00" };
        var color = colors[index % colors.Length];
        var shape = index switch
        {
            0 => "<rect x='120' y='40' width='160' height='170' rx='5'/><path d='M145 40v170M165 80h85M165 105h85M165 130h65' fill='none'/>",
            1 => "<path d='M95 165l25-125 18 4-25 125-18 20Z M175 175l20-130 18 3-20 130-18 20Z M255 175l14-125 18 2-14 125-18 20Z'/>",
            2 => "<path d='M160 205h110v15H140v-30h30l20-100-45-35 15-20 65 50-20 105h65v15Z'/><path d='M190 70l100 30-15 45-100-30Z'/>",
            3 => "<path d='M115 85h170l-15 135H130Z'/><path d='M155 85V65c0-50 90-50 90 0v20' fill='none' stroke-width='12'/>",
            4 => "<path d='M85 110h230l-20 100H105Z'/><path d='M85 110l35-40h160l35 40M105 180h190' fill='none'/>",
            5 => "<path d='M140 65h120l-12 150h-96Z'/><rect x='125' y='47' width='150' height='20'/><path d='M145 125h110v45H150Z' fill='#fff'/>",
            6 => "<rect x='155' y='60' width='90' height='160' rx='30'/><rect x='170' y='35' width='60' height='30'/><path d='M180 35V20h40v15' fill='none'/>",
            7 => "<rect x='95' y='75' width='210' height='130' rx='20'/><path d='M105 95h190M125 95v15m15-15v15m15-15v15m15-15v15m15-15v15m15-15v15m15-15v15m15-15v15m15-15v15' fill='none'/>",
            8 => "<path d='M100 200l65-150h120l-50 150Z'/><path d='M105 200h140v15H95M165 50l20 150M260 65l30 135' fill='none'/>",
            9 => "<rect x='90' y='95' width='220' height='35' rx='14'/><rect x='130' y='155' width='150' height='30' rx='12'/><circle cx='115' cy='112' r='5' fill='#fff'/><circle cx='150' cy='170' r='4' fill='#fff'/>",
            10 => "<path d='M95 65h200v125H95Z M110 190v15h200V80h-15' fill='none'/><path d='M115 165l40-55 35 35 35-65 50 85Z'/>",
            11 => "<path d='M120 65h160v145H120Z'/><path d='M120 65l80 65 80-65M120 210l65-75m95 75-65-75' fill='none'/><ellipse cx='200' cy='165' rx='13' ry='20' fill='#fff'/>",
            12 => "<rect x='70' y='100' width='260' height='55'/><path d='M90 100v25m20-25v15m20-15v25m20-25v15m20-15v25m20-25v15m20-15v25m20-25v15m20-15v25m20-25v15m20-15v25' fill='none'/>",
            13 => "<path d='M80 70q70-20 120 10 50-30 120-10v140q-70-20-120 10-50-30-120-10Z'/><path d='M200 80v140M100 100h65m-65 25h65m-65 25h65m75-50h60m-60 25h60' fill='none'/>",
            14 => "<path d='M130 130h140l-20 85H150Z'/><ellipse cx='200' cy='130' rx='70' ry='15'/><path d='M200 125V55m0 35q-60 5-45-40 45-5 45 40m0 10q60 5 45-40-45-5-45 40' fill='none' stroke-width='9'/>",
            15 => "<rect x='105' y='65' width='150' height='135'/><path d='M120 80h120m-120 15h120m-120 15h120m-120 15h120m-120 15h120m-120 15h120m-120 15h120M130 65v135m20-135v135m20-135v135m20-135v135m20-135v135m20-135v135' fill='none' stroke='#fff' stroke-width='3'/><path d='M255 80h25v135H130v-15' fill='none'/>",
            16 => "<path d='M145 60h115l-12 150-45-25-45 25Z'/><path d='M170 60V45h70v15m-60 35h45' fill='none'/>",
            17 => "<path d='M80 75h240v135H80Z'/><path d='M95 140h210v55H95ZM130 140V75m35 65V75m35 65V75m35 65V75m35 65V75' fill='none'/><path d='M310 95q40 25 0 50' fill='none'/>",
            18 => "<rect x='95' y='50' width='210' height='165'/><rect x='115' y='70' width='170' height='125' fill='#fff'/><path d='M140 95h60v35h-60Zm75 45h50v35h-50Z'/><circle cx='150' cy='90' r='5' fill='#111'/><circle cx='230' cy='135' r='5' fill='#111'/>",
            _ => "<path d='M110 60h180v155H110Z'/><path d='M110 95h180M145 45v30m110-30v30' fill='none' stroke-width='8'/><path d='M135 120h20v15h-20Zm40 0h20v15h-20Zm40 0h20v15h-20Zm40 0h15v15h-15ZM135 155h20v15h-20Zm40 0h20v15h-20Zm40 0h20v15h-20Z' fill='#fff'/>",
        };
        var sku = WebUtility.HtmlEncode($"ST-{index + 1:000}");
        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='400' height='260' viewBox='0 0 400 260'><rect width='400' height='260' fill='#fff'/><g fill='{color}' stroke='#111' stroke-width='4' stroke-linejoin='round'>{shape}</g><text x='16' y='245' font-family='sans-serif' font-size='13' fill='#111'>{sku} · ORIGINAL DEMO</text></svg>";
        return "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
    }
}
