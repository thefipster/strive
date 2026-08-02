// wwwroot/js/upload.js

const uploads = {}; // store controllers by uploadId

export async function startUploadFromInput(uploadId, dotNetRef) {
    // find the first file input on the page (simple approach)
    const input = document.querySelector('input[type="file"]');
    if (!input || input.files.length === 0) {
        dotNetRef.invokeMethodAsync('OnError', 'No file selected');
        return;
    }
    const file = input.files[0];

    // only allow .zip in this example
    if (!file.name.toLowerCase().endsWith('.zip')) {
        dotNetRef.invokeMethodAsync('OnError', 'Only .zip files are allowed');
        return;
    }

    const CHUNK_SIZE = 5 * 1024 * 1024; // 5MB chunks
    const totalSize = file.size;
    const totalChunks = Math.ceil(totalSize / CHUNK_SIZE);

    uploads[uploadId] = { canceled: false };

    let uploaded = 0;
    for (let i = 0; i < totalChunks; i++) {
        if (uploads[uploadId].canceled) {
            dotNetRef.invokeMethodAsync('OnError', 'Upload canceled by user');
            delete uploads[uploadId];
            return;
        }

        const start = i * CHUNK_SIZE;
        const end = Math.min(start + CHUNK_SIZE, totalSize);
        const blob = file.slice(start, end);

        // build form data
        const form = new FormData();
        form.append('chunk', blob, file.name);
        form.append('uploadId', uploadId);
        form.append('fileName', file.name);
        form.append('chunkIndex', i);
        form.append('totalChunks', totalChunks);
        form.append('totalSize', totalSize);

        // use XHR to get upload progress for this chunk
        try {
            await uploadChunkXHR('https://localhost:7098/api/upload/chunk', form, (evt) => {
                // evt.loaded is bytes uploaded for *this* chunk; we report overall uploaded
                const chunkUploaded = evt.loaded;
                const uploadedSoFar = uploaded + chunkUploaded;
                dotNetRef.invokeMethodAsync('OnProgress', uploadedSoFar, totalSize, file.name);
            });
            uploaded += (end - start);
            // after chunk uploaded, report progress (chunk completed)
            dotNetRef.invokeMethodAsync('OnProgress', uploaded, totalSize, file.name);
        } catch (err) {
            dotNetRef.invokeMethodAsync('OnError', err?.toString() ?? 'Upload failed');
            delete uploads[uploadId];
            return;
        }
    }

    // Completed
    dotNetRef.invokeMethodAsync('OnCompleted', file.name);
    delete uploads[uploadId];
}

function uploadChunkXHR(url, formData, onProgress) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open('POST', url, true);

        xhr.onload = function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response);
            } else {
                reject(`Server responded ${xhr.status}: ${xhr.responseText}`);
            }
        };

        xhr.onerror = function () {
            reject('Upload failed (network error)');
        };

        // onprogress on upload - gives progress for current chunk
        xhr.upload.onprogress = function (e) {
            if (e.lengthComputable && typeof onProgress === 'function') {
                onProgress(e);
            }
        };

        xhr.send(formData);
    });
}

export function cancelUpload(uploadId) {
    if (uploads[uploadId]) uploads[uploadId].canceled = true;
}