// Image Preview Functionality
(function () {
    'use strict';

    // Initialize image preview on page load
    document.addEventListener('DOMContentLoaded', function () {
        const imageUrlInput = document.getElementById('ImageUrl');
        if (!imageUrlInput) return;

        // Create preview container
        const previewContainer = document.createElement('div');
        previewContainer.id = 'imagePreviewContainer';
        previewContainer.className = 'image-preview-container mt-3';
        previewContainer.style.display = 'none';

        const previewImage = document.createElement('img');
        previewImage.id = 'imagePreview';
        previewImage.className = 'image-preview';
        previewImage.alt = 'Image preview';

        const removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.className = 'btn btn-sm btn-danger mt-2';
        removeButton.innerHTML = '<i class="fas fa-times me-1"></i>Remove Image';
        removeButton.onclick = function () {
            imageUrlInput.value = '';
            previewContainer.style.display = 'none';
        };

        previewContainer.appendChild(previewImage);
        previewContainer.appendChild(removeButton);
        imageUrlInput.parentNode.appendChild(previewContainer);

        // Load existing image if present
        if (imageUrlInput.value) {
            loadImagePreview(imageUrlInput.value, previewImage, previewContainer);
        }

        // Handle input changes
        let debounceTimer;
        imageUrlInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const url = this.value.trim();

            if (!url) {
                previewContainer.style.display = 'none';
                return;
            }

            // Debounce to avoid too many requests
            debounceTimer = setTimeout(function () {
                loadImagePreview(url, previewImage, previewContainer);
            }, 500);
        });
    });

    function loadImagePreview(url, imgElement, container) {
        // Validate URL format
        if (!isValidUrl(url)) {
            container.style.display = 'none';
            return;
        }

        // Show loading state
        imgElement.src = '';
        imgElement.alt = 'Loading...';
        container.style.display = 'block';

        // Create a temporary image to test loading
        const tempImg = new Image();

        tempImg.onload = function () {
            imgElement.src = url;
            imgElement.alt = 'Card image preview';
            container.style.display = 'block';
        };

        tempImg.onerror = function () {
            imgElement.src = '';
            imgElement.alt = 'Failed to load image';
            container.style.display = 'none';
        };

        tempImg.src = url;
    }

    function isValidUrl(string) {
        try {
            const url = new URL(string);
            return url.protocol === 'http:' || url.protocol === 'https:';
        } catch (_) {
            return false;
        }
    }
})();
