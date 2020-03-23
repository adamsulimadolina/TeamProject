document.addEventListener("DOMContentLoaded",function () {
    var AllFieldsJson;
    var fieldNames = [];
    var validSuperiorField = false; //poprawność wartości pola nadrzędnego i podrzędnych
    var validDependentField = false;
    console.log(document.getElementById("AllIndependentFieldsName").value);
    var allIndependentFieldsNames = JSON.parse(document.getElementById("AllIndependentFieldsName").value);
    console.log(allIndependentFieldsNames);

    $.ajax({
        url: "/api/FieldAPI",
        dataType: "json"
    }).done(res => AllFieldsJson = res);

    $(document).ajaxComplete(function () {

        for (let i = 0; i < AllFieldsJson.length; i++) {
            fieldNames.push(AllFieldsJson[i].name)
        }

        $("#SuperiorFieldName").autocomplete({
            source: fieldNames
        });
        $("#CurrentFieldName").autocomplete({
            source: allIndependentFieldsNames
        });
    });

    //dom manipulations
    $("#DependencyType").change(function (e) {
        if (e.target.value == "FieldVisibly") {
            $("#ActivationValueLabel").text("Podaj wartość aktywującą zależność:");
            $("#AddDependedField").show();
            $("#RelatedFieldsTable").show();
            $("#CurrentFieldName").autocomplete({
                source: allIndependentFieldsNames
            });
        }
        else {
            $("#ActivationValueLabel").text("Podaj maksymalną liczbę którą może przyjmować pole:");
            $("#AddDependedField").hide();
            $("#RelatedFieldsTable").hide();
            $("#CurrentFieldName").autocomplete({
                source: null
            });
        }
    });
});