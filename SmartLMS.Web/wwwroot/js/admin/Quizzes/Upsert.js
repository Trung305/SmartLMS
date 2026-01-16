

$(document).on('popup:loaded', function () {
    if ($('#createQuizForm').length > 0) {
        window.initQuizzesUpsert();
        $('#createQuizForm').removeData('validator');
        $('#createQuizForm').removeData('unobtrusiveValidation');

        $.validator.unobtrusive.parse($('#createQuizForm'));
    }
});

$(document).on('hidden.bs.modal', '#popupModal', function () {
    $('#createQuizForm').removeData('quizzes-initialized');
});

window.initQuizzesUpsert = function () {
    let questions = [];
    document.getElementById('IsTimedQuiz').addEventListener('change', function () {
        document.getElementById('timeLimitContainer').style.display = this.checked ? 'block' : 'none';
        updateSummary();
    });
    document.querySelectorAll('input[name="QuizTypeRadio"]').forEach(el => {
        el.addEventListener('change', function () {
            document.getElementById('lessonSelectorContainer').style.display = this.value === 'lesson' ? 'block' : 'none';
            updateSummary();
        });
    });
    document.querySelectorAll('#PassingScore, #MaxAttempts, #TimeLimit').forEach(el => {
        el.addEventListener('input', updateSummary);
    });
    document.querySelectorAll('input[name="questionType"]').forEach(radio => {
        radio.addEventListener('change', function () {
            const type = this.value;
            document.getElementById('answersSection').style.display = (type === '1' || type === '2') ? 'block' : 'none';
            document.getElementById('trueFalseSection').style.display = (type === '3') ? 'block' : 'none';
            document.getElementById('addAnswerBtn').style.display = (type === '1' || type === '2') ? 'block' : 'none';

            if (type === '3') {
                resetTrueFalseAnswers();
            } else if (type === '1' || type === '2') {
                resetMultipleAnswers(type);
            }
        });
    });
    document.querySelectorAll('input[name="questionType"]').forEach(radio => {
        radio.addEventListener('change', function () {
            const type = this.value;
            document.getElementById('answersSection').style.display = (type === '1' || type === '2') ? 'block' : 'none';
            document.getElementById('trueFalseSection').style.display = (type === '3') ? 'block' : 'none';
            document.getElementById('addAnswerBtn').style.display = (type === '1' || type === '2') ? 'block' : 'none';

            if (type === '3') {
                resetTrueFalseAnswers();
            } else if (type === '1' || type === '2') {
                resetMultipleAnswers(type);
            }
        });
    });
    document.querySelectorAll('#typeMultiple').forEach(cb => {
        cb.addEventListener('change', function () {
            if (this.checked) {
                resetMultipleAnswers(2);
            } else {
                resetMultipleAnswers(1);
            }
            
        });
    });
    window.openQuestionModal = function () {
        const modalEl = document.getElementById('questionModal');
        if (!modalEl) {
            console.log('Không tìm thấy #questionModal');
            return;
        }
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    };
    
    window.addAnswerOption = function (type = 1) {
        const list = document.getElementById('answersList');
        const div = document.createElement('div');
        div.className = 'answer-option';
        const inputType = type === '2' ? 'checkbox' : 'radio';
        div.innerHTML = `
        <input type="${inputType}" name="correctAnswer" class="form-check-input">
        <input type="text" class="form-control" placeholder="Nhập đáp án...">
        <button type="button" class="btn btn-sm btn-danger" onclick="this.parentElement.remove()">×</button>`;
        list.appendChild(div);
    };
    window.renderQuestions = function () {
        const container = document.getElementById('questionsList');
        container.innerHTML = "";
        document.getElementById('questionCount').textContent = questions.length;
        document.getElementById('summaryQuestions').textContent = questions.length;
        document.getElementById('summaryPoints').textContent = questions.reduce((s, q) => s + q.points, 0);

        if (questions.length === 0) {
            document.getElementById('emptyQuestionsState').style.display = 'block';
            return;
        }
        document.getElementById('emptyQuestionsState').style.display = 'none';

        questions.forEach((q, i) => {
            let badgeType = '';
            let icon = '';
            switch (q.type) {
                case '1': badgeType = 'info'; icon = 'dot-circle'; break;
                case '2': badgeType = 'primary'; icon = 'check-square'; break;
                case '3': badgeType = 'success'; icon = 'toggle-on'; break;
                case '4': badgeType = 'warning'; icon = 'align-left'; break;
            }

            let answersHtml = '';
            if (q.type === '1' || q.type === '2') {
                const inputType = q.type === '2' ? 'checkbox' : 'radio';
                answersHtml = '<div class="mt-3"><small class="text-muted">Đáp án:</small>';
                q.answers.forEach(a => {
                    answersHtml += `<div class="form-check"><input class="form-check-input" type="${inputType}" disabled ${a.correct ? 'checked' : ''}>
                    <label class="form-check-label ${a.correct ? 'text-success fw-bold' : ''}">${a.text} ${a.correct ? '✓' : ''}</label></div>`;
                });
                answersHtml += '</div>';
            } else if (q.type === '3') {
                answersHtml = `<small class="text-muted">Đáp án đúng: <strong class="${q.correctAnswer ? 'text-success' : 'text-danger'}">${q.correctAnswer ? 'Đúng' : 'Sai'}</strong></small>`;
            }

            const card = document.createElement('div');
            card.className = 'card question-card mb-3';
            card.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between">
                    <div>
                        <span class="badge bg-primary me-2">Câu ${i + 1}</span>
                        <span class="badge bg-${badgeType} me-2"><i class="fas fa-${icon} me-1"></i>
                            ${q.type === '1' ? 'Trắc nghiệm (Chọn 1)' : q.type === '2' ? 'Trắc nghiệm (Chọn nhiều)' : q.type === '3' ? 'Đúng/Sai' : 'Tự luận'}
                        </span>
                        <span class="badge bg-success">${q.points} điểm</span>
                        <p class="mt-3 mb-2 fw-bold">${q.content}</p>
                        ${q.explanation ? `<small class="text-muted d-block"><em>Giải thích: ${q.explanation}</em></small>` : ''}
                        ${answersHtml}
                    </div>
                    <div>
                        <button class="btn btn-sm btn-warning me-1" onclick="editQuestion(${i})"><i class="fas fa-edit"></i></button>
                        <button class="btn btn-sm btn-danger" onclick="deleteQuestion(${i})"><i class="fas fa-trash"></i></button>
                    </div>
                </div>
            </div>
        `;
            container.appendChild(card);
        });
    };
    function updateSummary() {
        const type = document.querySelector('input[name="QuizTypeRadio"]:checked').value === "lesson" ? "Gắn bài học" : "Tổng hợp";
        const passing = document.getElementById('PassingScore').value || 70;
        const attempts = document.getElementById('MaxAttempts').value || 3;
        const timed = document.getElementById('IsTimedQuiz').checked;
        const time = document.getElementById('TimeLimit').value;

        document.getElementById('summaryType').textContent = type;
        document.getElementById('summaryPassing').textContent = passing + '%';
        document.getElementById('summaryAttempts').textContent = attempts;
        document.getElementById('summaryTime').textContent = timed && time ? time + ' phút' : 'Không giới hạn';
        document.getElementById('summaryQuestions').textContent = questions.length;
        document.getElementById('summaryPoints').textContent = questions.reduce((sum, q) => sum + q.points, 0);
        document.getElementById('questionCount').textContent = questions.length;
    }
    function resetMultipleAnswers(type) {
        const list = document.getElementById('answersList');
        list.innerHTML = "";
        for (let i = 0; i < 4; i++) {
            addAnswerOption(type);
        }
    }

    function resetTrueFalseAnswers() {
        document.getElementById('tfTrue').checked = false;
        document.getElementById('tfFalse').checked = false;
    }

    window.saveQuestion = function () {
        let type = document.querySelector('input[name="questionType"]:checked').value;
        const content = document.getElementById('questionContent').value.trim();
        const points = parseInt(document.getElementById('questionPoints').value);
        const explanation = document.getElementById('questionExplanation').value.trim();
        const index = document.getElementById('editingIndex').value;
        if (document.querySelector('#typeMultiple').checked) {
            type = 2;
        }
        if (!content || !points) {
            alert('Vui lòng nhập nội dung câu hỏi và điểm số!');
            return;
        }

        let question = { type, content, points, explanation };

        if (type === '1' || type === '2') {
            const options = document.querySelectorAll('#answersList .answer-option');
            const answers = [];
            let hasCorrect = false;
            ;;
            let order = 1;
            options.forEach(opt => {
                const content = opt.querySelector('input[type="text"]').value.trim();
                const isCorrect = opt.querySelector('input[type="radio"], input[type="checkbox"]').checked;
                if (content) {
                    answers.push({ content, isCorrect, orderIndex: order++ });
                    if (isCorrect) hasCorrect = true;
                }
            });

            if (answers.length < 2 || !hasCorrect) {
                notion.warning('Cần ít nhất 2 đáp án và phải chọn ít nhất 1 đáp án đúng!');
                return;
            }
            question.answers = answers;
        } else if (type === '3') {
            const isCorrect = document.querySelector('input[name="trueFalseAnswer"]:checked')?.value;
            if (!isCorrect) {
                notion.warning('Vui lòng chọn Đúng hoặc Sai!');
                return;
            }
            question.isCorrect = isCorrect === 'true';
        }

        if (index !== '' && index >= 0) {
            questions[index] = question;
        } else {
            questions.push(question);
        }

        renderQuestions();
        bootstrap.Modal.getInstance(document.getElementById('questionModal')).hide();
    };

    function editQuestion(index) {
        const q = questions[index];
        document.getElementById('questionModalTitle').textContent = 'Chỉnh sửa câu hỏi';
        document.getElementById('editingIndex').value = index;
        document.getElementById('questionContent').value = q.content;
        document.getElementById('questionPoints').value = q.points;
        document.getElementById('questionExplanation').value = q.explanation || '';
        document.querySelector(`input[name="questionType"][value="${q.type}"]`).checked = true;

        document.querySelector(`input[name="questionType"][value="${q.type}"]`).dispatchEvent(new Event('change'));

        if (q.type === '3') {
            if (q.correctAnswer) document.getElementById('tfTrue').checked = true;
            else document.getElementById('tfFalse').checked = true;
        } else if (q.type === '1' || q.type === '2') {
            const list = document.getElementById('answersList');
            list.innerHTML = "";
            q.answers.forEach(a => {
                const inputType = q.type === '2' ? 'checkbox' : 'radio';
                const div = document.createElement('div');
                div.className = 'answer-option';
                div.innerHTML = `
                <input type="${inputType}" name="correctAnswer" class="form-check-input" ${a.correct ? 'checked' : ''}>
                <input type="text" class="form-control" value="${a.text}">
                <button type="button" class="btn btn-sm btn-danger" onclick="this.parentElement.remove()">×</button>
            `;
                list.appendChild(div);
            });
        }

        new bootstrap.Modal(document.getElementById('questionModal')).show();
    }

    function deleteQuestion(index) {
        if (confirm('Xóa câu hỏi này?')) {
            questions.splice(index, 1);
            renderQuestions();
        }
    }

    document.addEventListener('shown.bs.modal', function (e) {
        if (e.target && e.target.id === 'questionModal') {
            document.getElementById('questionModalTitle').textContent = 'Thêm câu hỏi mới';
            document.getElementById('editingIndex').value = -1;
            document.getElementById('questionContent').value = '';
            document.getElementById('questionPoints').value = '10';
            document.getElementById('questionExplanation').value = '';
            document.getElementById('typeSingle').checked = true;

            document
                .querySelector('input[name="questionType"][value="1"]')
                .dispatchEvent(new Event('change'));
        }
    });
    $(document).on('submit', '#createQuizForm', function (e) {
        e.preventDefault();
        if (questions.length === 0) {
            Swal.fire({
                title: 'Chưa có câu hỏi',
                text: 'Bài kiểm tra chưa có câu hỏi nào. Bạn có muốn lưu không?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Lưu',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    submitQuizForm();
                }
            });
            return;
        }

        submitQuizForm();
    });
    function submitQuizForm() {
        const $submitBtn = $('#createQuizForm button[type="submit"]');
        const originalText = $submitBtn.html();
        $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');

        const quizType = $('input[name="QuizTypeRadio"]:checked').val();
        const lessonId = quizType === 'lesson' ? $('#LessonIdSelect').val() : null;

        const data = {
            QuizId: $('#QuizId').val() || null,                          
            CourseId: $('#CourseId').val(),                              
            LessonId: lessonId,                                          
            Title: $('#Title').val().trim(),                             
            Description: $('#Description').val().trim() || null,         
            TimeLimit: parseInt($('#TimeLimit').val()) || 30,            
            MaxAttempts: parseInt($('#MaxAttempts').val()) || 3,         
            PassingScore: parseFloat($('#PassingScore').val()) || 70,    
            IsActive: $('#IsActive').is(':checked'),                     
            IsTimedQuiz: $('#IsTimedQuiz').is(':checked'),               
            Questions: questions || []                                   
        };
        const token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: $('#createQuizForm').attr('action'),
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': token  
            },
            success: function (response) {
                if (response.success) {
                    notion.success("Tạo bài kiểm tra thành công!", 4000);
                    showpopup(
                        'Quản lý bài học',
                        '/Admin/Lessons?courseId=' + response.courseId,
                    );

                    if (typeof $('#modalContainer') !== 'undefined') {
                        $('#modalContainer').modal('hide');
                    }
                } else {
                    notion.error('Có lỗi xảy ra!');
                    $submitBtn.prop('disabled', false).html(originalText);
                }
            },
            error: function (xhr) {
                $submitBtn.prop('disabled', false).html(originalText);
                notion.error('Có lỗi xảy ra! Vui lòng thử lại.');
            }
        });
    }
    function markInvalid(selector) {
        const $el = $(selector);
        $el.addClass('is-invalid');

        const $popup = $el.closest('#popupContent');
        if ($popup.length) {
            $popup.animate({
                scrollTop: $el.position().top - 60
            }, 200);
        }
    }

    function clearInvalid(formSelector) {
        $(formSelector).find('.is-invalid').removeClass('is-invalid');
    }
    
};
