// Narrow browser bridge: local text files, explicit downloads and clipboard only.
export async function readText(input, maximumBytes) {
    const file = input.files?.[0];
    if (!file) return null;
    if (file.size > maximumBytes) throw new Error("Choose a JSON file no larger than 4 MB.");
    const text = await file.text();
    input.value = "";
    return text;
}
export function download(name, text, type) {
    const url = URL.createObjectURL(new Blob([text], { type }));
    const link = document.createElement("a");
    link.href = url;
    link.download = name.replace(/[^A-Za-z0-9._-]/g, "-");
    document.body.append(link);
    link.click();
    link.remove();
    // The click has synchronously queued the navigation; retain the URL for that task.
    setTimeout(() => URL.revokeObjectURL(url), 0);
}
export function confirmReplace(message) { return window.confirm(message); }
export async function copy(text) {
    if (!navigator.clipboard?.writeText) return false;
    try { await navigator.clipboard.writeText(text); return true; }
    catch { return false; }
}
