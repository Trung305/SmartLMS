
$(document).on('submit', '#upsertCourseForm', function (e) {

    e.preventDefault();
    e.stopPropagation();

    if (!validateForm()) {
        notion.warning("Vui lòng kiểm tra lại các trường bắt buộc.", 4000);
        $('#popupContent').animate({ scrollTop: 0 }, 200);
        return false;
    }

    $('.is-invalid').removeClass('is-invalid');
    $('.text-danger').text('');
    $('#validationSummary').addClass('d-none');

    var form = $(this);
    var formData = new FormData(this);
    var submitBtn = $('#submitBtn');
    var btnText = $('#btnText');
    var originalText = btnText.html();
    var isEdit = $('#Id').val() !== '00000000-0000-0000-0000-000000000000';

    Swal.fire({
        title: isEdit ? 'Cập nhật khóa học?' : 'Tạo khóa học mới?',
        text: isEdit ? 'Bạn có chắc muốn cập nhật?' : 'Bạn có chắc muốn tạo mới?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: '<i class="fas fa-save"></i> ' + (isEdit ? 'Cập nhật' : 'Tạo'),
        cancelButtonText: '<i class="fas fa-times"></i> Hủy'
    }).then((result) => {
        if (result.isConfirmed) {

            submitBtn.prop('disabled', true);
            btnText.html('<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...');

            $.ajax({
                url: form.attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {

                    if (response.success) {
                        $('#popupModal').modal('hide');

                        notion.success(isEdit ? "Cập nhật khóa học thành công!": "Tạo khóa học thành công!", 4000);

                        if (typeof window.reloadCoursesList === 'function') {
                            setTimeout(function () {
                                window.reloadCoursesList();
                            }, 500);
                        }
                    } else {

                        submitBtn.prop('disabled', false);
                        btnText.html(originalText);

                        if (response.errors) {
                            displayValidationErrors(response.errors);

                            notion.warning("Vui lòng kiểm tra lại!", 4000);
                        } else {
                            notion.error("Có lỗi xảy ra, vui lòng liên hệ quản trị!", 4000);
                        }
                    }
                },
                error: function (xhr, status, error) {

                    submitBtn.prop('disabled', false);
                    btnText.html(originalText);

                    if (xhr.status === 401) {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    } else {
                        notion.error("Có lỗi xảy ra, vui lòng liên hệ quản trị!", 4000);
                    }
                }
            });
        }
    });

    return false;
});

$(document).on('change', '#isFree', function () {
    if ($(this).is(':checked')) {
        $('#priceInput').hide();
        $('#Price').val(0);
    } else {
        $('#priceInput').show();
    }
});

$(document).on('change', '#ThumbnailFile', function () {
    if (this.files && this.files[0]) {
        var file = this.files[0];
        if (file.size > 5 * 1024 * 1024) {
            setError('#ThumbnailFile', "File quá lớn! Vui lòng chọn ảnh nhỏ hơn 5MB");
            $(this).val('');
            return;
        }

        if (!file.type.match('image.*')) {
            setError('#ThumbnailFile', "File không hợp lệ! Chỉ chấp nhận file ảnh (JPG, PNG)");
            $(this).val('');
            return;
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            $('#thumbnailPreview').html(
                '<img src="' + e.target.result + '" class="img-fluid rounded" style="max-height: 200px;">' +
                '<small class="d-block text-muted mt-1">Ảnh mới (chưa lưu)</small>'
            );
            $('#currentThumbnail').hide();
        }
        reader.readAsDataURL(file);
    }
});

$(document).on('shown.bs.modal', '#popupModal', function () {

    if ($('#isFree').is(':checked')) {
        $('#priceInput').hide();
    }
});

function displayValidationErrors(errors) {
    $('#validationSummary').removeClass('d-none');
    $('#popupContent').animate({ scrollTop: 0 }, 300);

    $.each(errors, function (fieldName, errorMessage) {
        var $field = $('[name="' + fieldName + '"]');

        if ($field.length) {
            $field.addClass('is-invalid');

            var $errorSpan = $field.siblings('.text-danger');
            if ($errorSpan.length) {
                $errorSpan.text(errorMessage);
            } else {
                $field.after('<span class="text-danger">' + errorMessage + '</span>');
            }
        }
    });

    $('.is-invalid:first').focus();
}
function validateForm() {
    let isValid = true;

    $('.is-invalid').removeClass('is-invalid');
    $('.text-danger').text('');

    function setError(selector, message) {
        const field = $(selector);

        field.addClass("is-invalid");
        let errorSpan = field.closest(".form-group").find(".text-danger");
        if (errorSpan.length > 0) {
            errorSpan.text(message);
        } else {
            field.after('<span class="text-danger">' + message + '</span>');
        }
        $('#validationSummary').removeClass('d-none');
        $('#popupContent').animate({ scrollTop: 0 }, 300);

        isValid = false;
    }

    const title = $('#Title').val().trim();
    const shortDesc = $('#ShortDescription').val().trim();
    const desc = $('#Description').val().trim();
    const price = parseFloat($('#Price').val());
    const duration = parseInt($('#EstimatedDuration').val());
    const categoryId = parseInt($('#CategoryId').val());
    const isFree = $('#isFree').is(':checked');
    const createDate = $('#CreateDate').val();
    const endDate = $('#EndDate').val();

    const isEdit = $('#Id').val() !== '00000000-0000-0000-0000-000000000000';
    const fileInput = $('#ThumbnailFile')[0];


    if (!title) setError('#Title', "Tiêu đề là bắt buộc");
    if (!shortDesc) setError('#ShortDescription', "Mô tả ngắn là bắt buộc");
    if (!desc) setError('#Description', "Mô tả chi tiết là bắt buộc");

    if (!categoryId || categoryId <= 0) {
        setError('#CategoryId', "Vui lòng chọn danh mục");
    }

    if (isFree) {
        if (price !== 0) {
            $('#Price').val(0);
        }
    } else {
        if (isNaN(price) || price <= 0) {
            setError('#Price', "Giá phải lớn hơn 0");
        }
    }

    if (!isEdit && fileInput.files.length === 0) {
        setError('#ThumbnailFile', "Vui lòng chọn ảnh thumbnail");
    }

    if (fileInput.files.length > 0) {
        const file = fileInput.files[0];
        const validTypes = ["image/jpeg", "image/png", "image/jpg", "image/webp"];

        if (!validTypes.includes(file.type)) {
            setError('#ThumbnailFile', "Ảnh phải là JPG, PNG hoặc WEBP");
        }
    }

    return isValid;
}