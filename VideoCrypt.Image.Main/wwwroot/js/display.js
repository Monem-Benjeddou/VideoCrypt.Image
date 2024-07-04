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
            url: '@Url.Action("DeleteImage", "Image")',
            type: 'POST',
            data: { imageUrl: imageUrl },
            success: function (result) {
                if (result.success) {
                    location.reload(); // Refresh the page to reflect the changes
                } else {
                    alert(result.message);
                }
            }
        });
    }
}
