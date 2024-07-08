document.addEventListener('DOMContentLoaded', function() {
    var imageModal = document.getElementById('imageModal');
    if (!imageModal) {
        console.error("Element with ID 'imageModal' not found.");
        return;
    }

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

        // Attach click event listener to handle download
        downloadLink.addEventListener('click', function(event) {
            event.preventDefault(); // Prevent the default action (navigating to the image URL)
            downloadImage(imageSrc); // Call the downloadImage function with the image URL
        });
    });

    // Function to handle image download
    function downloadImage(imageUrl) {
        fetch(imageUrl)
            .then(response => response.blob())
            .then(blob => {
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                a.download = imageUrl.substring(imageUrl.lastIndexOf('/') + 1);
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            })
            .catch(() => alert('Failed to download image.'));
    }

    // Function to handle image deletion
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
});



