var imageModal = document.getElementById('imageModal');
imageModal.addEventListener('show.bs.modal', function (event) {
    var button = event.relatedTarget;
    var imageSrc = button.getAttribute('data-bs-src');

    var modalImage = imageModal.querySelector('#modalImage');
    modalImage.src = imageSrc;

    var downloadLink = imageModal.querySelector('#downloadLink');
    downloadLink.href = imageSrc;
});

function deleteImage(imageUrl) {
    if (confirm('Are you sure you want to delete this image?')) {
        $.ajax({
            url: '/Image/DeleteImage', // Ensure this matches your controller route
            type: 'POST',
            data: {imageUrl: imageUrl},
            success: function (result) {
                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error: " + status + " " + error);
            }
        });
    }
}
