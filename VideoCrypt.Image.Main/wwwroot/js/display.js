var imageModal = document.getElementById('imageModal');
imageModal.addEventListener('show.bs.modal', function (event) {
    var button = event.relatedTarget;
    var imageSrc = button.getAttribute('data-bs-src');
    var resolution = button.getAttribute('data-bs-resolution');
    var extension = button.getAttribute('data-bs-extension');

    var modalImage = imageModal.querySelector('#modalImage');
    modalImage.src = imageSrc;

    var imageResolution = imageModal.querySelector('#imageResolution');
    imageResolution.textContent = 'Resolution: ' + resolution;

    var imageExtension = imageModal.querySelector('#imageExtension');
    imageExtension.textContent = 'Extension: ' + extension;

    var downloadLink = imageModal.querySelector('#downloadLink');
    downloadLink.href = imageSrc; // Set the href attribute to the image source
});

function deleteImage(imageUrl) {
    if (confirm('Are you sure you want to delete this image?')) {
        $.ajax({
            url: '/Image/deleteImage',
            type: 'POST',
            data: {imageUrl: imageUrl},
            success: function (result) {
                if (result.success) {
                    $('#deleteSuccessModal').modal('show');
                    setTimeout(function () {
                        location.reload();
                    }, 2000);
                } else {
                    alert('Failed to delete image: ' + result.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error: " + status + " " + error);
                alert('Failed to delete image due to server error.');
            }
        });
    }
}


