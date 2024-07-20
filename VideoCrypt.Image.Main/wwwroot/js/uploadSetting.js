Dropzone.options.myGreatDropzone = {
    addRemoveLinks: true,
    paramName: "file",
    maxFiles: 1,
    acceptedFiles: "image/jpeg,image/png,image/gif,image/bmp,image/tiff,image/svg+xml",
    init: function () {
        var myDropzone = this;

        // Fetch existing files
        $.getJSON('./?handler=ListFolderContents').done(function (data) {
            if (data !== null && data.length > 0) {
                $.each(data, function (index, item) {
                    var mockFile = {
                        name: item.name,
                        size: item.fileSize,
                        filePath: item.filePath
                    };
                    myDropzone.emit("addedfile", mockFile);
                    myDropzone.emit("thumbnail", mockFile, item.filePath);
                    myDropzone.emit("complete", mockFile);
                    displayFileDetails(mockFile);
                    generateShareLink(mockFile.name);
                });
            }
        });

        // Handle added file
        myDropzone.on("addedfile", function (file) {
            if (myDropzone.files[1] != null) {
                myDropzone.removeFile(myDropzone.files[0]);
            }
        });

        // Handle success
        myDropzone.on("success", function (file, response) {
            displayFileDetails(file);
            generateShareLink(file.name);
        });

        // Handle removed file
        myDropzone.on("removedfile", function () {
            document.getElementById('fileDetails').innerHTML = '';
            document.getElementById('shareLink').innerHTML = '';
        });
    }
};

function displayFileDetails(file) {
    var fileDetails = document.getElementById('fileDetails');
    fileDetails.innerHTML = `
        <div class="file-detail-item">
            <strong>Name:</strong> <span>${file.name}</span>
        </div>
        <div class="file-detail-item">
            <strong>Size:</strong> <span>${formatFileSize(file.size)}</span>
        </div>
    `;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    var k = 1024,
        sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'],
        i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function generateShareLink(fileName) {
    fetch(`/Image/GenerateShareLink/${encodeURIComponent(fileName)}`)
        .then(response => response.json())
        .then(data => {
            var shareLinkDiv = document.getElementById('shareLink');
            shareLinkDiv.innerHTML = `
                <input type="text" class="share-link-input" value="${data.url}" readonly />
                <button class="btn btn-secondary btn-copy" onclick="copyToClipboard('${data.url}')">Copy Link</button>
            `;
        })
        .catch(error => {
            console.error('Error generating share link:', error);
        });
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .then(() => {
            alert('Link copied to clipboard!');
        })
        .catch(err => {
            console.error('Failed to copy text: ', err);
        });
}
