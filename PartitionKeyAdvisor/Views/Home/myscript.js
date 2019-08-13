
    $('.cardinality').DataTable({
        "paging": false
});
    $('.recommendation').DataTable({
        "paging": false,
    "bFilter": false
});


        $(function () {
            $('#selectedPartitionKey').change(function () {
                $('.shadow').hide();
                $('#' + $(this).val()).show();
            });
        });
function showDiv() {
    document.getElementById('loadingGif').style.display = "block";
    setTimeout(function () {
        document.getElementById('loadingGif').style.display = "none";
        document.getElementById('showme').style.display = "block";
    }, 30000);

}