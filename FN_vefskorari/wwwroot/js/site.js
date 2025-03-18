const baseUrl = window.location.origin;

// ✅ Upload File Function
window.uploadFile = async () => {
    let fileInput = document.getElementById("fileInput");
    let file = fileInput.files[0];

    if (!file) {
        alert("Please select a file.");
        return null;
    }

    let formData = new FormData();
    formData.append("file", file);

    try {
        let response = await fetch(`${baseUrl}/api/replay/upload`, {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            throw new Error("Upload failed: " + response.statusText);
        }

        let result = await response.json();
        document.getElementById("message").innerText = "File uploaded: " + result.fileName;
        return result.fileName; // Return file name to Blazor
    } catch (error) {
        console.error("Upload error:", error);
        alert("Upload failed. Check the console for details.");
        return null;
    }
};

// ✅ Fix Local Storage Access
// document.addEventListener("DOMContentLoaded", () => {
//     try {
//         localStorage.setItem("blazor-test", "test-value");
//         localStorage.removeItem("blazor-test");
//         console.log("✅ Local storage access confirmed.");
//     } catch (error) {
//         console.warn("⚠️ Local storage access is restricted:", error);
//     }
// });

// ✅ Fix MutationObserver to Avoid Errors
document.addEventListener("DOMContentLoaded", () => {
    try {
        const observer = new MutationObserver((mutations) => {
            console.log("DOM changed:", mutations);
        });

        const targetNode = document.body;
        if (targetNode) {
            observer.observe(targetNode, { childList: true, subtree: true });
        } else {
            console.warn("⚠️ MutationObserver: targetNode is null.");
        }
    } catch (error) {
        console.error("⚠️ MutationObserver error:", error);
    }
});

// ✅ Trigger File Download Function
window.triggerFileDownload = async (url) => {
    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorText = await response.text();
            console.error("Error downloading file:", errorText);
            alert("Error: " + errorText);
            return;
        }

        const blob = await response.blob();
        const contentType = blob.type;

        // Ensure we are getting an Excel file
        if (contentType !== "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
            console.error("Invalid file type:", contentType);
            alert("Error: The downloaded file is not a valid Excel file.");
            return;
        }

        // Trigger download
        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = "Leaderboard.xlsx";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

    } catch (error) {
        console.error("Download error:", error);
        alert("An error occurred while downloading the file. Check the console for details.");
    }
};

