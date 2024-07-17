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