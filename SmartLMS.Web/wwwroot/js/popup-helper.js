window.showpopup = function (title, link, onClose) {
    document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
        backdrop.remove();
    });

    if (!document.getElementById('popupModal')) {
        const modalHtml = `
<div class="modal fade" id="popupModal" tabindex="-1" aria-labelledby="popupModalLabel" aria-hidden="true">
  <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
    <div class="modal-content" style="max-height: calc(100vh - 3.5rem);">
      <div class="modal-header">
        <h5 class="modal-title" id="popupModalLabel"></h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button>
      </div>
      <div class="modal-body" style="overflow-y: auto; overflow-x: hidden;">
        <div id="popupContent">
          <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Đang tải...</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>`;
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        const modalElement = document.getElementById('popupModal');

        modalElement.addEventListener('hidden.bs.modal', function () {
            document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
                backdrop.remove();
            });
            if (!document.querySelector('.modal.show')) {
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }
            if (typeof modalElement._onCloseCallback === 'function') {
                modalElement._onCloseCallback();
            }
            modalElement._onCloseCallback = null;
        });
    }

    document.getElementById('popupModalLabel').innerText = title;
    const modalElement = document.getElementById('popupModal');
    modalElement._onCloseCallback = onClose || null;

    document.getElementById('popupContent').innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
        </div>`;

    fetch(link)
        .then(response => {
            if (response.redirected && response.url.includes('/Login')) {
                window.location.href = response.url;
                return null;
            }
            if (response.status === 401) {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                return null;
            }
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.text();
        })
        .then(html => {
            if (html) {
                $('#popupContent').html(html);
                initializePlugins();
                $(document).trigger('popup:loaded');
            }
        })
        .catch(error => {
            document.getElementById('popupContent').innerHTML = `
                <div class="alert alert-danger m-3">
                    <i class="bi bi-exclamation-triangle"></i>
                    Có lỗi xảy ra khi tải nội dung! Vui lòng thử lại.
                </div>`;
            console.error('Error:', error);
        });

    var popupModal = new bootstrap.Modal(document.getElementById('popupModal'));
    popupModal._onCloseCallback = onClose;
    popupModal.show();
}

function initializePlugins() {
    var forms = document.querySelectorAll('#popupContent form');
    Array.prototype.slice.call(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
        }, false);
    });
}