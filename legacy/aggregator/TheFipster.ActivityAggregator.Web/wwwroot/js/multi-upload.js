export async function startUpload(file, dotNetRef, uploadId) {
    const CHUNK_SIZE = 5 * 1024 * 1024; // 5MB
    const totalSize = file.size;
    const totalChunks = Math.ceil(totalSize / CHUNK_SIZE);

    let uploaded = 0;

    for (let i = 0; i < totalChunks; i++) {
        const start = i * CHUNK_SIZE;
        const end = Math.min(start + CHUNK_SIZE, totalSize);
        const blob = file.slice(start, end);

        const form = new FormData();
        form.append('chunk', blob, file.name);
        form.append('uploadId', uploadId);
        form.append('fileName', file.name);
        form.append('chunkIndex', i);
        form.append('totalChunks', totalChunks);
        form.append('totalSize', totalSize);

        await fetch('https://localhost:7098/api/upload/chunk', {
            method: 'POST',
            body: form
        });

        uploaded += (end - start);

        // report progress back to Blazor
        await dotNetRef.invokeMethodAsync('OnProgress', uploadId, uploaded, totalSize);
    }

    // completed
    await dotNetRef.invokeMethodAsync('OnCompleted', uploadId);
}