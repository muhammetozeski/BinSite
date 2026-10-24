# Web Analytics has a separate permission boundary

The credential used for BinSite successfully managed Pages publication and D1 schemas. A real request to create a Web Analytics site at `/accounts/{account}/rum/site_info` nevertheless returned Cloudflare error 10000, `Authentication error`.

The official create-site endpoint documents `Account Settings Write` as its accepted permission. Successful Pages or D1 administration is therefore not evidence that this separate analytics operation is authorized. The precise token grant was not inspected, and no analytics site or audience measurement was created by that request. Do not repeat the request with the unchanged credential expecting a different result, or report traffic as measured merely because Pages deployment succeeded.

Official references: https://developers.cloudflare.com/api/resources/rum/subresources/site_info/methods/create/ ; https://developers.cloudflare.com/web-analytics/get-started/ . The permission requirement is documentation evidence; the endpoint failure is an actual account observation from 24 October 2026.
