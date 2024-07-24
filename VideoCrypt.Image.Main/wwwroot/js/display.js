document.addEventListener('DOMContentLoaded', function() {
    let imageModal = document.getElementById('imageModal');
    if (!imageModal) {
        console.error("Element with ID 'imageModal' not found.");
        return;
    }
    imageModal.addEventListener('show.bs.modal', function(event) {
        let button = event.relatedTarget;
        let imageSrc = button.getAttribute('data-bs-src');
        let modalImage = imageModal.querySelector('#modalImage');
        let shareLink = imageModal.querySelector('#share-link');
        shareLink.value = imageSrc;
        modalImage.src = imageSrc;
        let imageResolution = imageModal.querySelector('#imageResolution');
        getMeta(imageSrc).then(img => {
            let resolution = `${img.naturalWidth}x${img.naturalHeight}`;
            imageResolution.textContent = 'Resolution: ' + resolution;
        }).catch(err => {
            imageResolution.textContent = 'Resolution: Unknown';
        });
        let imageExtension = imageModal.querySelector('#imageExtension');
        let extension = extractExtensionFromUrl(imageSrc);
        imageExtension.textContent = 'Extension: ' + (extension || 'Unknown');
        let downloadLink = imageModal.querySelector('#downloadLink');
        downloadLink.href = imageSrc;
        downloadLink.download = imageSrc.substring(imageSrc.lastIndexOf('/') + 1);
        downloadLink.onclick = function(e) {
            e.preventDefault();
            downloadImage(imageSrc);
        };
    });

    let copyLinkButton = document.getElementById('copyLinkButton');
    copyLinkButton.addEventListener('click', function() {
        let shareLink = document.getElementById('share-link');
        shareLink.select();
        shareLink.setSelectionRange(0, 99999); // For mobile devices
        document.execCommand('copy');
        alert('Copied the URL: ' + shareLink.value);
    });

    function downloadImage(imageUrl) {
        const apiUrl = `/Image/download?url=${encodeURIComponent(imageUrl)}`;

        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to download image.');
                }
                return response.blob();
            })
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                a.download = imageUrl.substring(imageUrl.lastIndexOf('/') + 1);
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            })
            .catch(error => alert(error.message));
    }

    function extractExtensionFromUrl(url) {
        return url.split('.').pop().split('?')[0];
    }

    const getMeta = (url) =>
        new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = (err) => reject(err);
            img.src = url;
        });
});

function deleteImage(imageUrl) {
    if (confirm('Are you sure you want to delete this image?')) {
        $.ajax({
            url: '/Image/deleteImage',
            type: 'POST',
            data: { imageUrl: imageUrl },
            success: function(result) {
                if (result.success) {
                    $('#deleteSuccessModal').modal('show');
                    setTimeout(function() {
                        location.reload();
                    }, 2000);
                } else {
                    alert('Failed to delete image: ' + result.message);
                }
            },
            error: function(xhr, status, error) {
                console.error("Error: " + status + " " + error);
                alert('Failed to delete image due to server error.');
            }
        });
    }
}
