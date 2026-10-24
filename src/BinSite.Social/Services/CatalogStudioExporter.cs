using System.Net;
using System.Text;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Builds a self-contained searchable catalog with encoded text, embedded local images and print styling.</summary>
public static class CatalogStudioExporter
{
    /// <summary>Exports included valid products; missing photos remain explicit placeholders.</summary>
    public static string Html(CatalogStudioProject project)
    {
        CatalogStudioWorkspace.EnsureShape(project);
        var issues = CatalogStudioWorkspace.Validate(project);
        var includedIds = project.Products.Where(product => product.Included).Select(product => product.Id).ToHashSet(StringComparer.Ordinal);
        var blocking = issues.Where(issue => issue.IsError && (issue.ProductId is null || includedIds.Contains(issue.ProductId))).ToArray();
        if (blocking.Length > 0) throw new FormatException($"Repair or exclude the {blocking.Length} blocking issue(s) before exporting.");
        if (includedIds.Count == 0) throw new FormatException("Include at least one product before exporting.");
        var products = project.Products.Where(product => product.Included).ToArray();
        var categories = products.Select(product => product.Category.Trim()).Where(category => category.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase);
        var html = new StringBuilder("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        html.Append("<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; img-src data:; style-src 'unsafe-inline'; script-src 'unsafe-inline'; base-uri 'none'; form-action 'none'\"><title>")
            .Append(Encode(project.Title)).Append("</title><style>").Append(Styles).Append("</style></head><body><header><h1>")
            .Append(Encode(project.Title)).Append("</h1><p class=\"description\">").Append(Encode(project.Description))
            .Append("</p></header><nav aria-label=\"Catalog filters\"><label>Search products<input id=\"search\" type=\"search\" placeholder=\"Name, code or description\"></label><label>Category<select id=\"category\"><option value=\"\">All categories</option>");
        foreach (var category in categories)
            html.Append("<option value=\"").Append(Encode(category)).Append("\">").Append(Encode(category)).Append("</option>");
        html.Append("</select></label><button type=\"button\" id=\"print\">Print results</button><output id=\"count\" aria-live=\"polite\"></output></nav><main id=\"products\">");
        foreach (var product in products)
        {
            var image = project.Images.FirstOrDefault(item => item.Id == product.ImageId && item.Attached);
            var searchText = string.Join(' ', product.Sku, product.Name, product.Category, product.Description);
            html.Append("<article class=\"product\" data-search=\"").Append(Encode(searchText)).Append("\" data-category=\"")
                .Append(Encode(product.Category.Trim())).Append("\">");
            if (image is not null)
                html.Append("<img src=\"").Append(Encode(image.DataUrl)).Append("\" alt=\"").Append(Encode(product.Name)).Append("\">");
            else html.Append("<div class=\"no-image\">No photo</div>");
            html.Append("<div class=\"product-body\"><p class=\"sku\">").Append(Encode(product.Sku)).Append("</p><h2>")
                .Append(Encode(product.Name)).Append("</h2><p class=\"category\">").Append(Encode(product.Category))
                .Append("</p><p class=\"description\">").Append(Encode(product.Description)).Append("</p><p class=\"price\">")
                .Append(Encode(CatalogStudioWorkspace.PriceLabel(product, project))).Append("</p>");
            if (product.Stock.Length > 0) html.Append("<p class=\"stock\">Stock: ").Append(Encode(product.Stock)).Append("</p>");
            html.Append("</div></article>");
        }
        html.Append("</main><p id=\"empty\" hidden>No products match these filters.</p><script>")
            .Append(Script).Append("</script></body></html>");
        return html.ToString();
    }

    /// <summary>Encodes author-controlled values for text and quoted HTML attributes.</summary>
    static string Encode(string value) => WebUtility.HtmlEncode(value);

    const string Styles = """
        *{box-sizing:border-box}body{color-scheme:dark;margin:0;background:#181f27;color:#edf2f7;font:16px system-ui,sans-serif}header,nav,main,footer,#empty{max-width:1250px;margin:auto;padding:22px}h1{font-size:30px;margin:0 0 12px}h2{font-size:20px;margin:8px 0}p{line-height:1.6}nav{display:flex;gap:18px;align-items:end;flex-wrap:wrap;border-top:1px solid #566777;border-bottom:1px solid #566777}label{display:grid;gap:7px;font-size:14px}input,select,button{font:inherit;border:1px solid #738696;border-radius:5px;background:#26343f;color:#edf2f7;padding:10px}input{min-width:220px;max-width:100%}button{cursor:pointer;background:#3e5e70;color:#f7fbff}button:disabled{cursor:default;background:#27333e;color:#bccbd7}button:focus-visible,input:focus-visible,select:focus-visible{outline:3px solid #b5d6e7;outline-offset:2px}main{display:grid;grid-template-columns:repeat(auto-fill,minmax(min(250px,100%),1fr));gap:22px}.product{border:1px solid #566777;background:#202b35;break-inside:avoid}.product img,.no-image{display:block;width:100%;height:230px;object-fit:contain;background:#fff;border-bottom:1px solid #566777}.no-image{display:grid;place-items:center;background:#26343f;color:#c2cdd7}.product-body{padding:18px}.sku,.category,.stock{font-size:13px;color:#c2cdd7}.price{font-size:21px;font-weight:700}.description{white-space:pre-wrap;overflow-wrap:anywhere}footer{border-top:1px solid #566777;font-size:13px;color:#c2cdd7}[hidden]{display:none}@media print{@page{size:A4;margin:13mm}nav,footer,#empty{display:none}header{padding:0 0 10mm}main{padding:0;grid-template-columns:repeat(2,minmax(0,1fr));gap:7mm}.product{background:#fff;border-color:#888;break-inside:avoid}.product img,.no-image{height:45mm;background:#fff;color:#444}.product-body{padding:4mm}body{color-scheme:light;background:#fff;color:#111;font-size:10pt}.sku,.category,.stock{color:#444}h1{font-size:22pt}h2{font-size:14pt}.price{font-size:13pt}}
        """;

    const string Script = """
        'use strict';const cards=[...document.querySelectorAll('.product')],search=document.getElementById('search'),category=document.getElementById('category'),print=document.getElementById('print');function update(){const terms=search.value.toLocaleLowerCase('en').trim().split(/\s+/).filter(Boolean);let count=0;for(const card of cards){const text=card.dataset.search.toLocaleLowerCase('en'),matches=terms.every(term=>text.includes(term))&&(!category.value||card.dataset.category.toLocaleLowerCase('en')===category.value.toLocaleLowerCase('en'));card.hidden=!matches;if(matches)count++;}document.getElementById('count').textContent=(count===cards.length?count:count+' of '+cards.length)+' products';document.getElementById('empty').hidden=count!==0;print.disabled=count===0;}search.addEventListener('input',update);category.addEventListener('change',update);print.onclick=async()=>{if(print.disabled)return;try{await Promise.all(cards.filter(card=>!card.hidden).flatMap(card=>[...card.querySelectorAll('img')]).map(image=>image.decode()));}catch{document.getElementById('count').textContent='A photo could not be loaded. Printing was stopped.';return;}window.print();};update();
        """;
}
