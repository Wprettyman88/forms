// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function veryifyOrderElementsExist() {    
    // Verify elements exist
    if (!printingFilter) {
        console.error('printing-filter element not found!');
    }
    if (!printingLoading) {
        console.error('printing-loading element not found!');
    }
    if (!printingError) {
        console.error('printing-error element not found!');
    }
}