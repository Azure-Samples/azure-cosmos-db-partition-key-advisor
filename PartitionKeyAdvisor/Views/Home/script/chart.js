

    var randomColorPlugin = {

        // We affect the `beforeUpdate` event
        beforeUpdate: function(myChart) {
        var backgroundColor = [];
    var borderColor = [];
        for (var i = 0; i < myChart.config.data.datasets[0].data.length; i++) {
            var color = "rgba(" + Math.floor(Math.random() * 255) + "," + Math.floor(Math.random() * 255) + "," + Math.floor(Math.random() * 255) + ",";
    borderColor.push(color + "1)");
}
myChart.config.data.datasets[0].backgroundColor = backgroundColor;
myChart.config.data.datasets[0].borderColor = borderColor;
}
};
Chart.pluginService.register(randomColorPlugin);


var ctx = document.getElementById('myChart').getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
        labels: i,
            datasets: [{
        label: '# of Votes',
    data: j,
    backgroundColor: 'rgba(54, 162, 235, 0.2)',
    borderColor: 'rgba(54, 162, 235, 1)',
    borderWidth: 1
}]
},
        options: {
        title: {
        display: true,
        text: 'Unique of Properties',
        fontSize: '30',
        fontFamily: "'Roboto', sans-serif",
        fontColor: '#000'

     },
            scales: {
        yAxes: [{
        ticks: {
        beginAtZero: true
}
}]
}
}
});
