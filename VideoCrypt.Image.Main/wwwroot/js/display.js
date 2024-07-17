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
        downloadLink.target = '_blank';
        downloadLink.download = imageSrc.substring(imageSrc.lastIndexOf('/') + 1);
    });

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
                        location.reload(); // Reload the component instead of the whole page
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