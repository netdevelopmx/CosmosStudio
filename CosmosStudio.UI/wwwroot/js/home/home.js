$(document).ready(function () {
    // Define the base query for selecting items
    const baseQuery = `SELECT TOP 1000 * FROM c`;
    var databaseId = "";
    var containerId = "";
    var endpointUri = "";
    var primaryKey = "";
    // Function to create a collapsible tree structure
    function buildDatabaseContainerTree(databasesAndContainers) {
        var treeHtml = '<ul class="tree-view">';
        $.each(databasesAndContainers, function (databaseId, containers) {
            treeHtml += `
            <li class="database-node">
                <span class="toggle-icon">[+]</span> 
                <span class="database-icon">${databaseId}</span>
                <ul class="container-list" style="display: none;">`;
            $.each(containers, function (index, containerId) {
                treeHtml += `
                <li class="container-node" data-database-id="${databaseId}" data-container-id="${containerId}">
                    <span class="container-icon">${containerId}</span>
                </li>`;
            });
            treeHtml += '</ul></li>';
        });
        treeHtml += '</ul>';
        $('#databaseContainerTree').html(treeHtml);

        // Click handlers for expand/collapse
        $('.toggle-icon').click(function () {
            var containersList = $(this).siblings('.container-list');
            containersList.toggle();

            // Change icon based on visibility
            $(this).text(containersList.is(':visible') ? '[-]' : '[+]');
        });
    }

    // ... rest of your code for connecting and error handling


    // Connect to Cosmos DB and load containers
    $('#connectButton').click(function () {
        var endpointUri = $('#EndpointUri').val();
        var primaryKey = $('#PrimaryKey').val();

        $.ajax({
            url: '/Home/GetAllDatabasesAndContainers',
            type: 'POST',
            data: {
                EndpointUri: endpointUri,
                PrimaryKey: primaryKey
            },
            success: function (data) {
                buildDatabaseContainerTree(data);
            },
            error: function (xhr) {
                console.error('Error fetching databases and containers:', xhr.responseText);
            }
        });
    });

    // Handler for database node click to toggle container list
    

    // Handler for container node click to fetch and display items
    $(document).on('click', '.container-node', function () {
        databaseId = $(this).data('database-id');
        containerId = $(this).data('container-id');
        endpointUri = $('#EndpointUri').val();
        primaryKey = $('#PrimaryKey').val();
        selectContainer(databaseId, containerId, endpointUri, primaryKey, baseQuery);
        $("#queryInput").val(baseQuery); // Reset the query input (optional")
    });

    // Function to create a query for a selected container and display results
    function selectContainer(databaseId, containerId, endpointUri, primaryKey, query) {
        $.ajax({
            url: '/Home/GetContainerItems',
            type: 'GET',
            data: {
                EndpointUri: endpointUri,
                PrimaryKey: primaryKey,
                DatabaseId: databaseId,
                ContainerId: containerId,
                Query: query
            },
            success: function (result) {
                $('#itemsResultSection').html(result);

                $('.table-responsive .table').DataTable({
                    // DataTables options can go here
                });
            },
            error: function (xhr) {
                console.log('Error:', xhr.responseText);
            }
        });
    }
    $(document).on('click', '.detail-toggle', function () {
        $(this).next('.sub-entity-details').toggle();
    });

    //
    $(document).on('click', '#btnRunQuery', function () {
        var query = $('#queryInput').val();
        selectContainer(databaseId, containerId, endpointUri, primaryKey, query);

        editor.set("");
    });

    $(document).on('click', '#saveDocument', function () {
        // Assuming editor is already initialized and contains the updated JSON document
        var updatedJson = editor.get();
        var documentId = updatedJson.id; // or a method to retrieve the document ID from the editor or your UI

        // Validate the existence of required data before making the AJAX call
        if (!documentId || !endpointUri || !primaryKey || !databaseId || !containerId) {
            alert('All parameters must be provided to save the document.');
            return;
        }

        var docdata = new FormData();
        // Append values from the concrete request form to the FormData object
        docdata.append("endpointUri", endpointUri);
        docdata.append("primaryKey", primaryKey);
        docdata.append("databaseId", databaseId);
        docdata.append("containerId", containerId);
        docdata.append("id", documentId);
        docdata.append("updatedDocument", JSON.stringify(updatedJson));

        $.ajax({
            url: '/Home/UpdateDocument', // Your API endpoint that corresponds to the controller action
            type: 'POST',
            data: docdata,
            processData: false,  // Important! This tells jQuery not to process the data
            contentType: false,  // Important! This tells jQuery not to set the content type
            success: function (response) {
                selectContainer(databaseId, containerId, endpointUri, primaryKey, baseQuery);
                alert('Document saved successfully!');
            },
            error: function (xhr, status, error) {
                alert('Error occurred: ' + error);
            }
        });
    });

    $(document).on('click', '.delete-btn', function () {
        var docId = $(this).data('doc-id');

        if (!docId) {
            alert('Document ID is required.');
            return;
        }

        if (!confirm('Are you sure you want to delete this document?')) {
            return;
        }
        var docdata = new FormData();
        // Append values from the concrete request form to the FormData object
        docdata.append("endpointUri", endpointUri);
        docdata.append("primaryKey", primaryKey);
        docdata.append("databaseId", databaseId);
        docdata.append("containerId", containerId);
        docdata.append("id", docId);
         // Make the AJAX call to the server to delete the document
        $.ajax({
            url: '/Home/DeleteDocument',
            type: 'POST',
            processData: false, // Prevent jQuery from processing the data
            contentType: false, // Prevent jQuery from setting the contentType
            data: docdata,
            success: function (response) {
                alert('Document deleted successfully.');
                // Optionally, refresh the table or remove the row from the DOM
            },
            error: function (xhr, status, error) {
                alert('Error deleting document: ' + error);
            }
        });

    });


    // Clear the connection settings
    $('#clearButton').click(function () {
        clearConnectionFields();
    });

    // Function to clear connection fields and container list
    function clearConnectionFields() {
        $('#EndpointUri').val('');
        $('#PrimaryKey').val('');
        // Assuming there is an input for DatabaseId based on your script
        $('#DatabaseId').val('');
        $('#databaseContainerTree').empty(); // Clear the tree view
    }



    //JSON


    var container = document.getElementById("jsoneditor");
    var options = {
        mode: 'code',
        modes: ['code', 'form', 'text', 'tree', 'view'], // Allow all modes
        onChange: function () {
            // You can perform actions here whenever the JSON changes, like enabling the save button
            $('#saveDocument').prop('disabled', false);
        }
    };
    var editor = new JSONEditor(container, options);

    // Function to populate the editor with a JSON document
    function populateEditor(jsonData) {
        editor.set(jsonData);
    }

    // Save button click event
    $(document).on('click', '#saveDocument', function () {
    
        var updatedJson = editor.get();
        // Here you would add the AJAX call to your API to save the updated JSON
        // Make sure to validate and handle the JSON properly in your API to prevent security issues
    });


    
    $(document).on('click', 'table.table-hover tbody tr', function () {
        var jsonData = $(this).data('json'); // Assuming you have stored the JSON data in a data attribute
        editor.set(jsonData)
    });



});
