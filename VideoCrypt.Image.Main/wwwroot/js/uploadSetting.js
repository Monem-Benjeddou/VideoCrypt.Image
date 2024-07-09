Dropzone.options.myGreatDropzone = {
    addRemoveLinks: true,
    paramName: "file",
    maxFiles: 1,
    acceptedFiles: "image/jpeg,image/png,image/gif,image/bmp,image/tiff,image/svg+xml", 
    init: function () {
        var myDropzone = this;
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
                    displayFileDetails(mockFile, myDropzone);
                    showGenerateLinkButton();
                });
            }
        });

        myDropzone.on("addedfile", function (file) {
            if (myDropzone.files[1] != null) {
                myDropzone.removeFile(myDropzone.files[0]);
            }
        });

        myDropzone.on("success", function (file, response) {
            displayFileDetails(file, myDropzone);
            showGenerateLinkButton();
        });

        myDropzone.on("removedfile", function (file) {
            document.getElementById('fileDetails').innerHTML = '';
            document.getElementById('shareLink').innerHTML = '';
        });
    }
};

function displayFileDetails(file, dropzoneInstance) {
    var fileDetails = document.getElementById('fileDetails');
    fileDetails.innerHTML = '<h3>File Details</h3>' +
        '<p><strong>Name:</strong> <span id="fileName">' + file.name + '</span></p>' +
        '<p><strong>Size:</strong> ' + formatFileSize(file.size) + '</p>' ;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    var k = 1024,
        sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'],
        i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function showGenerateLinkButton() {
    var fileNameElement = document.querySelector('#fileName');
    if (!fileNameElement) return;

    var generateLinkButton = document.createElement('button');
    generateLinkButton.id = 'generateLinkButton';
    generateLinkButton.className = 'btn btn-primary';
    generateLinkButton.innerText = 'Generate Share Link';
    generateLinkButton.addEventListener('click', function () {
        var fileName = fileNameElement.innerText;
        fetch(`/UploadFile/GenerateShareLink/${encodeURIComponent(fileName)}`)
            .then(response => response.json())
            .then(data => {
                var shareLinkDiv = document.getElementById('shareLink');
                shareLinkDiv.innerHTML = '<button class="share-link-button btn btn-success" onclick="window.open(\'' + data.url + '\', \'_blank\')">Open Share Link</button>';
            })
            .catch(error => {
                console.error('Error generating share link:', error);
            });
    });

    var fileDetails = document.getElementById('fileDetails');
    fileDetails.appendChild(generateLinkButton);
}
