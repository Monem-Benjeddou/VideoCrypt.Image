var imageModal = document.getElementById('imageModal');
imageModal.addEventListener('show.bs.modal', function (event) {
    var button = event.relatedTarget;
    var imageSrc = button.getAttribute('data-bs-src');

    var modalImage = imageModal.querySelector('#modalImage');
    modalImage.src = imageSrc;

    var downloadLink = imageModal.querySelector('#downloadLink');
    downloadLink.href = imageSrc;

    var imageUrlInput = imageModal.querySelector('#imageUrl');
    imageUrlInput.value = imageSrc;
});
