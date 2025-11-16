// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

// Focus element by ID (for auto-focus on login)
window.focusElement = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

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

// Scroll element to top (for List Review person selection)
window.scrollElementToTop = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
};

// Christmas Activity Chart - Initialize timeline with item creation/claim data
window.initChristmasActivityChart = function(canvasId, createdData, claimedData, currentDateIndex, thanksgivingIndex, blackFridayIndex, christmasIndex) {
    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded!');
        return null;
    }

    // Register annotation plugin if available
    if (typeof ChartAnnotation !== 'undefined') {
        Chart.register(ChartAnnotation);
    }

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found:', canvasId);
        return null;
    }

    const ctx = canvas.getContext('2d');

    // Get theme colors from CSS variables
    const rootStyles = getComputedStyle(document.documentElement);
    const lightBlueColor = '#87CEEB'; // Winter light blue (sky blue)
    const greenColor = rootStyles.getPropertyValue('--color-success').trim() || '#2D5F4E';
    const accentColor = rootStyles.getPropertyValue('--color-accent').trim() || '#D6001C';
    const goldColor = rootStyles.getPropertyValue('--color-secondary').trim() || '#D4AF37'; // Orange for Thanksgiving
    const grayColor = rootStyles.getPropertyValue('--color-gray-400').trim() || '#CED4DA';
    const darkColor = rootStyles.getPropertyValue('--color-gray-800').trim() || '#343A40';
    const blackColor = '#000000'; // Black for Today

    // Build annotation configuration with holiday markers
    const annotations = {};

    // Black Friday shading (vertical bar behind the chart)
    if (blackFridayIndex >= 0) {
        annotations.blackFridayShading = {
            type: 'box',
            xMin: blackFridayIndex - 0.5,
            xMax: blackFridayIndex + 0.5,
            backgroundColor: darkColor + '50', // Darker shading (50 = ~30% opacity)
            borderWidth: 0,
            drawTime: 'beforeDatasetsDraw' // Draw behind the data
        };
    }

    // Thanksgiving line (gold/orange)
    if (thanksgivingIndex >= 0) {
        annotations.thanksgivingLine = {
            type: 'line',
            xMin: thanksgivingIndex,
            xMax: thanksgivingIndex,
            borderColor: goldColor,
            borderWidth: 2,
            borderDash: [3, 3],
            label: {
                display: true,
                content: 'Thanksgiving',
                position: 'end',
                yAdjust: 5,
                backgroundColor: goldColor,
                color: '#fff',
                font: {
                    family: 'Inter, sans-serif',
                    size: 10,
                    weight: '600'
                },
                padding: 3
            }
        };
    }

    // Today line (black)
    if (currentDateIndex >= 0) {
        annotations.todayLine = {
            type: 'line',
            xMin: currentDateIndex,
            xMax: currentDateIndex,
            borderColor: blackColor,
            borderWidth: 2,
            borderDash: [5, 5],
            label: {
                display: true,
                content: 'Today',
                position: 'end',
                yAdjust: 5,
                backgroundColor: blackColor,
                color: '#fff',
                font: {
                    family: 'Inter, sans-serif',
                    size: 11,
                    weight: '600'
                },
                padding: 4
            }
        };
    }

    // Christmas line
    if (christmasIndex >= 0) {
        annotations.christmasLine = {
            type: 'line',
            xMin: christmasIndex,
            xMax: christmasIndex,
            borderColor: accentColor,
            borderWidth: 3,
            label: {
                display: true,
                content: '🎄 Christmas',
                position: 'end',
                yAdjust: 5,
                backgroundColor: accentColor,
                color: '#fff',
                font: {
                    family: 'Inter, sans-serif',
                    size: 11,
                    weight: '700'
                },
                padding: 5
            }
        };
    }

    const annotationConfig = Object.keys(annotations).length > 0 ? {
        annotation: { annotations }
    } : {};

    try {
        const chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: createdData.labels,
                datasets: [
                    {
                        label: 'Items Created',
                        data: createdData.values,
                        borderColor: lightBlueColor,
                        backgroundColor: lightBlueColor + '30', // 30 = ~20% opacity in hex
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2,
                        pointRadius: 0,
                        pointHoverRadius: 4,
                        pointHoverBackgroundColor: lightBlueColor,
                    },
                    {
                        label: 'Items Claimed',
                        data: claimedData.values,
                        borderColor: greenColor,
                        backgroundColor: greenColor + '30',
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2,
                        pointRadius: 0,
                        pointHoverRadius: 4,
                        pointHoverBackgroundColor: greenColor,
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 3,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 15,
                            font: {
                                family: 'Inter, sans-serif',
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        titleFont: {
                            family: 'Inter, sans-serif',
                            size: 13,
                            weight: '600'
                        },
                        bodyFont: {
                            family: 'Inter, sans-serif',
                            size: 12
                        },
                        displayColors: true,
                        callbacks: {
                            title: function(context) {
                                return context[0].label;
                            }
                        }
                    },
                    ...annotationConfig
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: {
                                family: 'Inter, sans-serif',
                                size: 11
                            },
                            maxRotation: 45,
                            minRotation: 0
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: grayColor + '40',
                            drawBorder: false
                        },
                        ticks: {
                            font: {
                                family: 'Inter, sans-serif',
                                size: 11
                            },
                            precision: 0
                        }
                    }
                }
            }
        });

        return chart;
    } catch (error) {
        console.error('Error creating chart:', error);
        return null;
    }
};
