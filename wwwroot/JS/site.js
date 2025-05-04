export function setupContactButtons() {
    setTimeout(() => {
        const phoneButton = document.querySelector('.phone-button');
        const emailLink = document.querySelector('.email-link');
        
        if (phoneButton) {
            phoneButton.addEventListener('click', handlePhoneClick);
        }

        if (emailLink) {
            emailLink.addEventListener('click', handleEmailClick);
        }
    }, 300);
}

function handlePhoneClick(e) {
    if (!isMobileDevice()) {
        e.preventDefault();
        togglePhoneNumber(this);
    }
}

function togglePhoneNumber(button) {
    const phoneText = button.querySelector('.phone-text');
    const fullPhone = button.querySelector('.full-phone');
    
    // Verificação adicional de null
    if (phoneText && fullPhone) {
        phoneText.style.display = phoneText.style.display === 'none' ? 'inline' : 'none';
        fullPhone.style.display = fullPhone.style.display === 'none' ? 'inline' : 'none';
        
        if (fullPhone.style.display === 'inline') {
            setTimeout(() => {
                if (phoneText && fullPhone) {
                    phoneText.style.display = 'inline';
                    fullPhone.style.display = 'none';
                }
            }, 3000);
        }
    }
}

function isMobileDevice() {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

function handleEmailClick(e) {
    const link = e.currentTarget;

    if (!isMobileDevice()) {
        e.preventDefault();
        toggleEmailAddress(link);
    } else {
        const email = link.dataset.email;
        if (email) {
            openEmailClient(email);
        }
    }
}

function toggleEmailAddress(link) {
    const emailText = link.querySelector('.email-text');
    const fullEmail = link.querySelector('.full-email');

    if (emailText && fullEmail) {
        emailText.style.display = emailText.style.display === 'none' ? 'inline' : 'none';
        fullEmail.style.display = fullEmail.style.display === 'none' ? 'inline' : 'none';

        if (fullEmail.style.display === 'inline') {
            setTimeout(() => {
                emailText.style.display = 'inline';
                fullEmail.style.display = 'none';
            }, 3000);
        }
    }
}

window.openEmailClient = function(email) {
    try {
        const mailtoLink = document.createElement('a');
        mailtoLink.href = `mailto:${email}`;
        mailtoLink.style.display = 'none';
        document.body.appendChild(mailtoLink);
        mailtoLink.click();
        document.body.removeChild(mailtoLink);
    } catch (error) {
        console.error('Erro ao abrir cliente de email:', error);
        window.location.href = `mailto:${email}`;
    }
};