// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

// Claim/Unclaim animation triggers
window.triggerClaimAnimation = function(itemId) {
    const card = document.getElementById('item-card-' + itemId);
    if (card) {
        // Add the animation class
        card.classList.add('item-card-claiming');

        // Remove the animation class after it completes so it can be triggered again
        setTimeout(() => {
            card.classList.remove('item-card-claiming');
        }, 600);
    }
};

window.triggerUnclaimAnimation = function(itemId) {
    const card = document.getElementById('item-card-' + itemId);
    if (card) {
        // Add the animation class
        card.classList.add('item-card-unclaiming');

        // Remove the animation class after it completes so it can be triggered again
        setTimeout(() => {
            card.classList.remove('item-card-unclaiming');
        }, 400);
    }
};
