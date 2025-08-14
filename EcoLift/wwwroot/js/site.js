// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Enhanced Navbar scroll effect with smooth transitions
document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.querySelector('.navbar');
    
    if (navbar) {
        // Add transition class for smooth effects
        navbar.classList.add('navbar-transition');
        
        window.addEventListener('scroll', function() {
            if (window.scrollY > 10) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
        
        // Trigger on page load to handle refresh scenarios
        if (window.scrollY > 10) {
            navbar.classList.add('scrolled');
        }
    }
    
    // Enhanced button hover effects
    const actionButtons = document.querySelectorAll('.search-btn, .create-ride-btn');
    actionButtons.forEach(button => {
        // Remove transform effects, keep other hover enhancements if needed
        // button.addEventListener('mouseenter', function() {
        //     this.style.transform = 'translateY(-2px)';
        // });
        
        // button.addEventListener('mouseleave', function() {
        //     this.style.transform = 'translateY(0)';
        // });
    });
    
    // Smooth dropdown menu animations using Bootstrap events
    const dropdownMenus = document.querySelectorAll('.profile-dropdown-menu');
    dropdownMenus.forEach(menu => {
        // Listen for Bootstrap's dropdown show event
        menu.addEventListener('show.bs.dropdown', function() {
            this.style.opacity = '0';
            this.style.transform = 'translateY(-10px)';
            setTimeout(() => {
                this.style.opacity = '1';
                this.style.transform = 'translateY(0)';
            }, 10);
        });
        
        // Listen for Bootstrap's dropdown hide event
        menu.addEventListener('hide.bs.dropdown', function() {
            this.style.opacity = '0';
            this.style.transform = 'translateY(-10px)';
        });
    });
});
