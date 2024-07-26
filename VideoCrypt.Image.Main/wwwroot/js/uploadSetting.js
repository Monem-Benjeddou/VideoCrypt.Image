Dropzone.options.myGreatDropzone = {
    addRemoveLinks: true,
    paramName: "file",
    maxFiles: 1,
    acceptedFiles: "image/jpeg,image/png,image/gif,image/bmp,image/tiff,image/svg+xml",
    init: function () {
        let myDropzone = this;
        $.getJSON('./?handler=ListFolderContents').done(function (data) {
            if (data !== null && data.length > 0) {
                $.each(data, function (index, item) {
                    // Function to extract the file extension
                    function getFileExtension(filename) {
                        const lastDotIndex = filename.lastIndexOf('.');
                        return (lastDotIndex === -1 || lastDotIndex === filename.length - 1)
                            ? ''
                            : filename.substring(lastDotIndex + 1);
                    }
                    let extension = getFileExtension(item.name);
                    let fileName = generateQuickGuid() + (extension ? '.' + extension : '');
                    let mockFile = {
                        name: fileName,
                        size: item.fileSize,
                        filePath: item.filePath
                    };
                    myDropzone.emit("addedfile", mockFile);
                    myDropzone.emit("thumbnail", mockFile, item.filePath);
                    myDropzone.emit("complete", mockFile);
                    updatePreviewTemplate(mockFile, item.filePath);
                });
            }
        });

        myDropzone.on("addedfile", function (file) {
            if (myDropzone.files[1] != null) {
                myDropzone.removeFile(myDropzone.files[0]);
            }
        });

        myDropzone.on("success", function (file, response) {
            updatePreviewTemplate(file, file.name);
        });
        myDropzone.on("removedfile", function (file) {
            let dropZone =document.getElementById('my-great-dropzone');
            if(dropZone){
                dropZone.style.removeProperty('border')
            }
            let shareLink = document.querySelector("dz-share-link");
            if(shareLink){
                shareLink.innerHTML = '';
            }
        });
    }
};
function generateQuickGuid() {
    return Math.random().toString(36).substring(2, 15) +
        Math.random().toString(36).substring(2, 15);
}
function updatePreviewTemplate(file, filePath) {
    let previewElement = file.previewElement;
    let progressBar = previewElement.querySelector(".dz-progress")
    if (progressBar) {
        progressBar.remove();
    }
    if (previewElement) {
        let shareLinkInput = previewElement.querySelector('.share-link-input');
        let copyButton = previewElement.querySelector('.btn-copy');

        fetch(`/Image/GenerateShareLink/${encodeURIComponent(file.name)}`)
            .then(response => response.json())
            .then(data => {
                if (shareLinkInput) {
                    shareLinkInput.value = data.url;
                }
                if (copyButton) {
                    copyButton.setAttribute('onclick', `copyToClipboard(event, '${data.url}')`);
                }
            })
            .catch(error => {
                console.error('Error generating share link:', error);
            });
    }
}

function copyToClipboard(event, text) {
    event.preventDefault();  
    navigator.clipboard.writeText(text)
        .then(() => {
            alert('Link copied to clipboard!');
        })
        .catch(err => {
            console.error('Failed to copy text: ', err);
        });
}
