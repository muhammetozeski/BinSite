// File and image transport only. Catalog parsing, validation and export composition live in C#.
export async function readText(input, maximumBytes) {
    const file = input.files && input.files[0];
    if (!file) return null;
    try {
        if (file.size > maximumBytes) throw new Error("This file is too large to open.");
        return { name: file.name, text: await file.text() };
    } finally {
        input.value = "";
    }
}

function decodeImage(url) {
    return new Promise((resolve, reject) => {
        const image = new Image();
        image.onload = () => resolve(image);
        image.onerror = () => reject(new Error("This image could not be opened."));
        image.src = url;
    });
}

async function fingerprint(file) {
    const bytes = new Uint8Array(await file.arrayBuffer());
    if (globalThis.crypto && crypto.subtle) {
        const digest = await crypto.subtle.digest("SHA-256", bytes);
        return "sha256:" + [...new Uint8Array(digest)].map(value => value.toString(16).padStart(2, "0")).join("");
    }
    // A rebinding hint, never an authentication or integrity guarantee.
    let value = 2166136261;
    for (const byte of bytes) value = Math.imul(value ^ byte, 16777619) >>> 0;
    return "local-hint:" + bytes.length + ":" + value.toString(16);
}

export async function readImages(input) {
    const files = [...(input.files || [])], result = { images: [], errors: [] };
    try {
        if (files.length > 80) throw new Error("Select at most 80 images per batch.");
        for (const file of files) {
            let url = "";
            try {
                if (!["image/png", "image/jpeg", "image/webp"].includes(file.type))
                    throw new Error("Use a PNG, JPEG or WebP file.");
                if (file.size > 8 * 1024 * 1024) throw new Error("The file exceeds 8 MB.");
                url = URL.createObjectURL(file);
                const image = await decodeImage(url);
                if (!image.naturalWidth || !image.naturalHeight) throw new Error("The image has no readable dimensions.");
                const scale = Math.min(1, 1200 / Math.max(image.naturalWidth, image.naturalHeight));
                const canvas = document.createElement("canvas");
                canvas.width = Math.max(1, Math.round(image.naturalWidth * scale));
                canvas.height = Math.max(1, Math.round(image.naturalHeight * scale));
                const context = canvas.getContext("2d");
                if (!context) throw new Error("The browser could not create an image canvas.");
                context.fillStyle = "#ffffff";
                context.fillRect(0, 0, canvas.width, canvas.height);
                context.drawImage(image, 0, 0, canvas.width, canvas.height);
                const dataUrl = canvas.toDataURL("image/jpeg", 0.86);
                if (dataUrl.length > 2800000) throw new Error("The resized image is still too large.");
                result.images.push({
                    id: globalThis.crypto?.randomUUID ? crypto.randomUUID().replaceAll("-", "") : "local-" + Date.now() + "-" + Math.random().toString(36).slice(2),
                    name: file.name, dataUrl, width: canvas.width, height: canvas.height,
                    fingerprint: await fingerprint(file)
                });
            } catch (error) {
                result.errors.push(file.name + ": " + (error instanceof Error ? error.message : "Image processing failed."));
            } finally {
                if (url) URL.revokeObjectURL(url);
            }
        }
        return result;
    } finally {
        input.value = "";
    }
}

export function download(name, text, contentType) {
    const safeName = name.replace(/[\\/:*?"<>|]/g, "-").slice(0, 120);
    const url = URL.createObjectURL(new Blob([text], { type: contentType }));
    const link = document.createElement("a");
    link.href = url;
    link.download = safeName;
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export function printPreview(html) {
    const preview = window.open("", "_blank");
    if (!preview) {
        download("catalog-print-preview.html", html, "text/html;charset=utf-8");
        return false;
    }
    preview.opener = null;
    preview.document.open();
    preview.document.write(html);
    preview.document.close();
    return true;
}
