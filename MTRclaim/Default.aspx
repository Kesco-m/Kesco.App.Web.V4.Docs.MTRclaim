<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Kesco.App.Web.Docs.MTRСlaim.Default"%>
<%@ Register TagPrefix="cc" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="cc1" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head runat="server">
    <title></title>

    <script>

        // добавляем окну свойство перемещения
        $(function() {
            $("#addEditDialog").draggable({ containment: "window", handle: "#dlgHeader", cursor: "move" });
        });

        $(function() {
            $("#DeleteDialog").draggable({ containment: "window", handle: "#DivDel", cursor: "move" });
        });

        $(function() {
            $("#LinksDialog").draggable({ containment: "window", handle: "#LinksHeader", cursor: "move" });
        });

        $(function() {
            $("#LinkConfirm").draggable({ containment: "window", handle: "#LinkConfHeader", cursor: "move" });
        });

        // показать в архиве документа
        function OpenDoc(docId) {
            cmd("cmd", "ShowInDocView", "DocId", docId);
        }

        // скрыть диалог добавления/редактирования позиции(показываю из серверного скрипта)
        function HidePositionDlg() {
            var dialog = document.getElementById("addEditDialog");

            if (dialog.style.display !== "none") {
                dialog.style.display = "none";
            }
        }

        // скрыть диалог добавления связаных документов
        function HideLinksDlg() {
            var dialog = document.getElementById("LinksDialog");

            if (dialog.style.display !== "none") {
                dialog.style.display = "none";
            }
        }

        // скрыть диалог подтверждения связывания
        function HideLinkConfirmDlg() {
            var dialog = document.getElementById("LinkConfirm");

            if (dialog.style.display !== "none") {
                dialog.style.display = "none";
            }
        }


        // показывает диалог и центрирует его
        function ShowMtrDlg() {
            var dlg = gi("addEditDialog");
            dlg.style.display = "block";

            $("#addEditDialog").position({
                my: "center",
                at: "center",
                of: window
            });
        }

        // показать диалог связывания, и показывает левее от события инициализации
        function ShowLinksDialog() {
            var near = $("#LinksDialog");
            var w = near.width() * 2;
            near.css({
                top: event.pageY,
                left: event.pageX - w
            });
            gi("LinksDialog").style.display = "block";
        }

        // var docAllowDrop = 0;

        function SetDragInfo(dropDocId) {
            event.dataTransfer.setData("Text", dropDocId.toString());
        }

        function GetDropInfo(pos) {
            // совместимость с IE 8
            (event.preventDefault) ? event.preventDefault() : event.returnValue = false;
            //event.preventDefault();
            var dd = event.dataTransfer.getData("Text");
            if (dd > 0)
                cmd("cmd", "LinkDroped", "mtrPos", pos, "DropDocId", dd);
        }

        function allowDrop() {
            // совместимость с IE 8
            (event.preventDefault) ? event.preventDefault() : event.returnValue = false;
            // event.preventDefault();
        }

    </script>

    <style>
        .inl { display: inline-table; }

        .al { vertical-align: bottom; }

        .wd {
            vertical-align: top;
            width: 100px;
        }

        .dialogWd {
            vertical-align: top;
            width: 150px;
        }

        .ctlMargMain { margin: 0px 0px 2px 0px }

        .ctlMargin { margin: 5px 5px 0 5px; }

        .clHover:hover { background-color: rgba(34, 137, 221, 1); }

        .block { display: none; }

        .holder:hover .block { display: inline; }

        .brdTop { border-top: 5px solid gainsboro; }

        .brdBottom { border-bottom: 5px solid gainsboro; }

        .brdRight { border-left: 5px solid gainsboro; }

        .brdLeft { border-right: 5px solid gainsboro; }
    </style>

</head>

<body>
<%= RenderDocumentHeader() %>
<br/>

<div class="v4FormContainer">

<div class="ctlMargMain">
    <% RenderDocNumDateNameRows(Response.Output); %>
</div>
<!--наличие отсутствие руководителя-->
<div id="headDivision" style="color: red; float: right; text-align: right;"></div>

<div class="ctlMargMain" style="display: block;">
    <div class="inl wd unselectable"><%= GetLocalizationFieldName(Mtr.Organization) %></div>
    <div class="inl al">
        <cc:DBSPerson ID="Company" runat="server" Width="334" IsCaller="True" Filter-PersonType="1" CallerType="Person" OnChanged="CompanyChanged" NextControl="Subdivision"></cc:DBSPerson>
    </div>
</div>

<div class="ctlMargMain">
    <div class="inl wd"><%= GetLocalizationFieldName(Mtr.Subdivision) %></div>
    <div class="inl al">
        <cc:DBSSubdivision ID="Subdivision" runat="server" Width="334" OnChanged="SubdivisionChanged" NextControl="PerformerOfSubdivision"></cc:DBSSubdivision>
    </div>
</div>

<div class="ctlMargMain">
    <div class="inl wd"><%= GetLocalizationFieldName(Mtr.PerformerOfSubdivision) %></div>
    <div class="inl al">
        <cc:DBSEmployee ID="PerformerOfSubdivision" runat="server" Width="334" IsCaller="True" CallerType="Employee" OnChanged="PerformerChanged" OnBeforeSearch="PerformerOfSubdivision_OnBeforeSearch" NextControl="DBSDocBasis"></cc:DBSEmployee>
    </div>
</div>
<br/>
<div class="ctlMargMain">
    <div class="inl wd"><%= GetLocalizationFieldName(Mtr.Basis) %></div>
    <div class="inl al">
        <cc:DBSDocument ID="DBSDocBasis" runat="server" Width="334" IsMultiSelect="True" IsRemove="True" ConfirmRemove="True" NextControl="txaDocDesc" OnChanged="DBSDocBasis_OnChanged" OnDeleted="DBSDocBasis_OnDeleted"></cc:DBSDocument>
    </div>
</div>

<br/>

<%--Блок диалога--%>
<div id="addEditDialog" onkeydown=" if (event.keyCode === 27) {HidePositionDlg();} " style="BACKGROUND-COLOR: white; border: 4px double black; display: none; position: absolute; width: 445px;">
    <div id="dlgHeader" class="gridHeader" style="background: #F1F1F1; border-bottom: darkgray 1px solid; height: 15px; margin-bottom: 10px; text-align: center; width: 100%;">
        <span id="dlgHeaderText"></span>
        <img src="/styles/cancel.gif" border="0" align="right" onclick=" HidePositionDlg(); ">
    </div>
    <div style="display: block;">
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblMtrName %>:</div>
            <div class="inl al">
                <cc1:TextBox ID="txbMtrName" runat="server" Width="250" maxlength="300" IsRequired="True" NextControl="txbSpecifications"></cc1:TextBox>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblSpecifications %>:</div>
            <div class="inl al">
                <cc1:TextBox ID="txbSpecifications" runat="server" Width="250" maxlength="300" IsRequired="True" NextControl="txaPurposeOfAcquisition"></cc1:TextBox>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblPurposeOfAcquisition %>:</div>
            <div class="inl al">
                <cc1:TextArea ID="txaPurposeOfAcquisition" runat="server" Width="250" MaxLength="500" IsRequired="True" NextControl="dpPurchasesTerm"></cc1:TextArea>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblPurchasesTerm %>:</div>
            <div class="inl al">
                <cc1:DatePicker ID="dpPurchasesTerm" runat="server" IsRequired="True" NextControl="txbUnit"></cc1:DatePicker>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"> <%= lblUnit %>:</div>
            <div class="inl al">
                <cc1:TextBox ID="txbUnit" runat="server" Width="100" maxlength="10" IsRequired="True" NextControl="txbQuantity"></cc1:TextBox>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblQuantity %>:</div>
            <div class="inl al">
                <cc1:Number ID="txbQuantity" runat="server" Width="100" IsRequired="True" NextControl="txaDescription"></cc1:Number>
            </div>
        </div>
        <div class="ctlMargin">
            <div class="inl dialogWd"><%= lblNote %>:</div>
            <div class="inl al">
                <cc1:TextArea ID="txaDescription" runat="server" Width="400" MaxLength="500" NextControl="btnAction"></cc1:TextArea>
            </div>
        </div>
    </div>
    <br/>
    <div id="buttons" style="float: left">
        <div style="float: left; padding: 5px">
            <cc1:Button ID="btnAction" runat="server" Style="BACKGROUND: buttonface url(/Styles/Save.gif) no-repeat left center; Width: 120px"></cc1:Button>
        </div>
        <div style="float: left; padding: 5px">
            <cc1:Button ID="btnCancel" runat="server" OnClick="HidePositionDlg();" Style="BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center; Width: 90px"></cc1:Button>
        </div>
    </div>

    <div>
        <div>
            <cc1:Changed runat="server" ID="chChanged"></cc1:Changed>
        </div>
    </div>

</div>

<!--Форма удаления-->
<div id="DeleteDialog" style="BACKGROUND-COLOR: white; border: 4px double black; display: none; left: 50%; position: absolute; top: 50%;">
    <div id="DivDel" style="background: #F1F1F1; background-color: #629ed0; border-bottom: darkgray 1px solid; border-right: darkgray 1px solid; height: 15px; margin-bottom: 10px; text-align: center; width: 100%;">
        <%= lblRemoval %>
    </div>
    <div id="divDelMessage" style="font-weight: bolder; padding: 10px; text-align: center;"></div>
    <br/>
    <div style="margin-left: 20%;">
        <div style="float: left; padding: 5px">
            <cc1:Button ID="btnDelete" runat="server" Text="Удалить" Style="BACKGROUND: buttonface url(/Styles/Delete.gif) no-repeat left center; Width: 120px"></cc1:Button>
        </div>
        <div style="float: left; padding: 5px">
            <cc1:Button ID="btnDelCancel" runat="server" OnClick="gi('DeleteDialog').style.display = 'none'" Text="Отмена" Style="BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center; Width: 90px"></cc1:Button>
        </div>
    </div>
</div>

<!--Форма добавления связей -->
<div id="LinksDialog" onkeydown=" if (event.keyCode === 27) {HideLinksDlg();} " style="BACKGROUND-COLOR: white; border: 4px double black; display: none; position: absolute; width: 280px;">
    <div id="LinksHeader" class="gridHeader" style="background: #F1F1F1; border-bottom: darkgray 1px solid; height: 15px; margin-bottom: 10px; text-align: center; width: 100%;">
        <span id="LinksHeaderText" style="cursor: move">Основания платежа</span>
        <img src="/styles/cancel.gif" border="0" align="right" onclick=" HideLinksDlg(); ">
    </div>
    <div style="padding-left: 10px;">

        <div class="ctlMargMain">
            <div class="inl wd"><%= "Документ: " %></div>
            <div class="inl al">
                <cc:DBSDocument ID="DBSDocToLink" runat="server" Width="230" IsRemove="True" OnChanged="DocToLink_OnChanged" OnBeforeSearch="DBSDocToLink_OnBeforeSearch"></cc:DBSDocument>
            </div>
        </div>

        <br/>
        <br/>

        <div id="linkPayBasis"><% RenderLinksDialogTables(Response.Output); %></div>

        <br/>
    </div>
</div>

<!--Форма подтверждения связывания-->

<div id="LinkConfirm" onkeydown=" if (event.keyCode === 27) {HideLinkConfirmDlg();} " style="BACKGROUND-COLOR: white; border: 4px double black; display: none; position: absolute; width: 280px;">
    <div id="LinkConfHeader" class="gridHeader" style="background: #F1F1F1; border-bottom: darkgray 1px solid; height: 15px; margin-bottom: 10px; text-align: center; width: 100%;">
        <span id="Span1">Связывание документа</span>
        <img src="/styles/cancel.gif" border="0" align="right" onclick=" HideLinkConfirmDlg(); ">
    </div>

    <div style="text-align: center">Хотите связать документ с позицией?</div>

    <div style="padding-left: 10px;">

        <div id="CheckedPositions"></div>

        <div id="Div1" style="float: left">
            <div style="float: left; padding: 5px">
                <cc1:Button ID="btnConfLnk" runat="server" Text="Сохранить" Style="BACKGROUND: buttonface url(/Styles/Save.gif) no-repeat left center; Width: 120px"></cc1:Button>
            </div>
            <div style="float: left; padding: 5px">
                <cc1:Button ID="btnConfCancel" runat="server" Text="Отмена" OnClick="HideLinkConfirmDlg();" Style="BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center; Width: 90px"></cc1:Button>
            </div>
        </div>

    </div>
</div>


<div>
    <div><%= GetLocalizationFieldName(Mtr.RequestItems) %></div>
    <div id="divPositions"><% RenderPositions(Response.Output); %></div>
</div>

<br/>
<br/>

<% StartRenderVariablePart(Response.Output); %>

<br/>

<div>
    <div id="NOTDistributedDocs"><% RenderPayBasisNOTDistributed(Response.Output, true); %></div>
</div>

<div>
    <div id="StoreDistributed">
        <% RenderStoreDistributed(Response.Output); %>
    </div>
</div>

<% EndRenderVariablePart(Response.Output); %>
<br/>
<br/>


<%--    <div>
            <% RenderLinkedDocsInfo(Response.Output); %>
        </div>--%>
</div>
</body>
</html>