using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.MTR;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.MTRclaim
{
    /// <summary>
    ///  Основная страница
    /// </summary>
    public partial class Default : DocPage
    {
        /// <summary>
        ///  Текущий типизированный документ
        /// </summary>
        public MTRClaim Mtr { get { return (MTRClaim)Doc; } }


        private static string lblPosCol2;
        private static string lblUnitShort;
        private static string lblPosCol10;
        private static string MsgDocs;
        private static string lblAddition;
        private static string lblEdit;
        private static string cmdCopy;
        
        protected static string ppBtnCancel;
        
        protected static string lblMtrName;
        protected static string lblSpecifications;
        protected static string lblPurposeOfAcquisition;
        protected static string lblPurchasesTerm;
        protected static string lblUnit;
        protected static string lblQuantity;
        protected static string lblNote;
        protected static string lblRemoval;

        protected static string lblPartialPay;

        
        #region Override

        /// <summary>
        /// Обработчик события загрузки страницы
        /// </summary>
        /// <param name="sender">Страница</param>
        /// <param name="e">Параметры</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                Company.Focus();

                Subdivision.Filter.PcId.CompanyHowSearch = "0";

                if (!CurrentPerson.IsNullEmptyOrZero())
                {
                    Subdivision.Filter.PcId.Value = Company.Value = CurrentPerson;
                    DBSDocBasis.Filter.PersonIDs.Value = CurrentPerson;

                    var personId = CurrentPerson.ToInt();
                    Mtr.Organization.Value =  personId;

                    if (CurrentUser.OrganizationId == personId)
                    {
                        Mtr.Subdivision.Value = Subdivision.Value = Employee.GetUserDivision(CurrentUser.EmployeeId);
                        PerformerOfSubdivision.Value = CurrentUser.Id;
                        Mtr.PerformerOfSubdivision.Value = CurrentUser.EmployeeId;

                        SetHeadDivisionText();
                    }
                }
                else if (Doc.IsNew && CurrentUser.OrganizationId != null)
                {
                    Subdivision.Filter.PcId.Value = Company.Value = CurrentUser.OrganizationId.ToString();
                    DBSDocBasis.Filter.PersonIDs.Value = CurrentUser.OrganizationId.ToString();
                    Mtr.Organization.Value = CurrentUser.OrganizationId;
                    Mtr.Subdivision.Value = Subdivision.Value = Employee.GetUserDivision(CurrentUser.EmployeeId);
                    PerformerOfSubdivision.Value = CurrentUser.Id;
                    Mtr.PerformerOfSubdivision.Value = CurrentUser.EmployeeId;

                    SetHeadDivisionText();
                }

               

                lblQuantity = Resx.GetString("lblQuantity");
                lblMtrName = Resx.GetString("lblMtrName");
                lblSpecifications = Resx.GetString("lblSpecifications");
                lblPurposeOfAcquisition = Resx.GetString("lblPurposeOfAcquisition");
                lblPurchasesTerm = Resx.GetString("lblPurchasesTerm");
                lblUnit = Resx.GetString("lblUnit");
                lblNote = Resx.GetString("lblNotes");

                lblPosCol2 = Resx.GetString("lblPosCol2");
                lblUnitShort = Resx.GetString("lblUnitShort");
                lblPosCol10 = Resx.GetString("lblPosCol10");
                MsgDocs = Resx.GetString("msgDocs");
                Resx.GetString("listFollowType");
                Resx.GetString("lblPayment");
                Resx.GetString("ppFltOsn");
                Resx.GetString("lblCargowaybills");
                lblAddition = Resx.GetString("lblAddition");
                lblEdit = Resx.GetString("lblEdit");
                lblRemoval = Resx.GetString("lblRemoval");
                cmdCopy = Resx.GetString("cmdCopy");

                btnCancel.Text = btnDelCancel.Text = Resx.GetString("ppBtnCancel");
                btnAction.Text = Resx.GetString("cmdSave");
                btnDelete.Text = Resx.GetString("btnDelete");

                lblPartialPay = Resx.GetString("lblPartialPay");
            }
        }

        /// <summary>
        /// Обработка клиентских команд
        /// </summary>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            var order = param["mtrPos"].ToInt();
            switch (cmd)
            {
                case "AddEditDialog":
                    if (order > 0)
                        ShowEditDialog(order);
                    else
                        ShowAddDialog();
                    break;
                case "CopyDialog":
                    ShowCopyDialog(order);
                    break;
                case "DeleteDialog":
                    ShowDeleteDialog(order);
                    break;
                case "AddEditPosition":
                    if (order > 0)
                        EditPosition(order);
                    else
                        AddPosition();
                    break;
                case "DeletePosition":
                    DeletePosition(order);
                    break;
                case "UpPosition":
                    UpperPosition(order);
                    break;
                case "DownPosition":
                    DownPosition(order);
                    break;
                case "LinkDroped":
                    var docId = param["DropDocId"].ToInt();
                    DocDropedAction(order, docId);
                    break;
                case "DocLinked":
                    var ldocId = param["DropDocId"].ToInt();
                    DocLinkedAction(ldocId);
                    break;
                case "RemoveLinkDoc":
                    var lnkDocId = param["LnkDocId"].ToInt();
                    var ask = param["ask"].ToBool();
                    LinkedDocDelete(order, lnkDocId, ask);
                    break;
                case "LinkWithMtr":
                    var mtrLnkDocId = param["MtrLnkDocId"].ToInt();
                    ConfirmDocToLinkWithMtr(false, mtrLnkDocId);
                    break;

                case "PartialPay":
                    int numValue = param["numVal"].ToInt();
                    int partDropDocId = param["DropDocId"].ToInt();
                    PartialPayChanged(numValue, partDropDocId);
                    break;

                // еще один post нужен для совместной работы(comet)
                case "RefreshPositions":
                    RefreshPositions();
                    break;

                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Инициализация конкретного документа
        /// </summary>
        /// <param name="copy">Параметр указывается если копируем документ</param>
        protected override void DocumentInitialization(Document copy = null)
        {
            if (copy == null)
                Doc = new MTRClaim();
            else
                Doc = (MTRClaim)copy;

            Doc.Date = DateTime.Today;
            ShowDocDate = false;
        }

        /// <summary>
        ///  Установить параметры контролов: параметры, дефолтные значения и т.д.
        /// </summary>
        protected override void SetControlProperties()
        {
            Company.IsRequired = Mtr.Organization.IsMandatory;
            Subdivision.IsRequired = Mtr.Subdivision.IsMandatory;
            PerformerOfSubdivision.IsRequired = Mtr.PerformerOfSubdivision.IsMandatory;

            DBSDocBasis.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Mtr.Basis.DocFieldId));
            DBSDocBasis.ConfirmRemoveMsg = Resx.GetString("msgOsnAttention7");

            // счета
            DBSDocToLink.Filter.Type.DocTypeParams.Add(new DocTypeParam { DocTypeID = ((int)DocTypeEnum.Счет).ToString(), QueryType = DocTypeQueryType.Equals});
            DBSDocToLink.Filter.Type.DocTypeParams.Add(new DocTypeParam { DocTypeID = ((int)DocTypeEnum.ИнвойсПроформа).ToString(), QueryType = DocTypeQueryType.Equals });

            // договора
            DBSDocToLink.Filter.Type.DocTypeParams.Add(new DocTypeParam { DocTypeID = ((int)DocTypeEnum.Договор).ToString(), QueryType = DocTypeQueryType.WithChildren });

            // приложения
            DBSDocToLink.Filter.Type.DocTypeParams.Add(new DocTypeParam { DocTypeID = ((int)DocTypeEnum.Приложение).ToString(), QueryType = DocTypeQueryType.WithChildren });

            if (!Mtr.IsNew)
            {
                DBSDocToLink.Filter.Date.Value = Mtr.Date.ToSqlDate();
                DBSDocToLink.Filter.Date.DateSearchType = DateSearchType.MoreThan;
            }

            SetHeadDivisionText();

            if(IsInDocView)
                DocumentReadOnly = true;
            else if(!DocEditable)
                DocumentReadOnly = true;
        }

        protected override void LoadData(string id)
        {
            base.LoadData(id);

            if (!id.IsNullEmptyOrZero())
            {
                Mtr.ReloadPositions();
                Mtr.BasisDocLinks = DocLink.LoadBasisDocsByChildId(Doc.DocId);
                Mtr.PositionDocLinks = MtrChildDoc.GetAllLinkedDocs(Doc.DocId);
            }
        }

        /// <summary>
        ///  Метод проверки на режим только чтение
        /// </summary>
        protected override void OnSignChanged()
        {
           base.OnSignChanged();

           #region установка режима только чтение
            if (IsInDocView)
                DocumentReadOnly = true;
            else if (!DocEditable)
                DocumentReadOnly = true;
            else
                DocumentReadOnly = false;

           #endregion

            SetHeadDivisionText();
        }

        private bool DocumentReadOnly
        {
            set
            {
                Company.IsReadOnly = value;
                Subdivision.IsReadOnly = value;
                PerformerOfSubdivision.IsReadOnly = value;
                DBSDocBasis.IsReadOnly = value;
            }
        }

        /// <summary>
        /// Обновляет поля специфичные для данного документа(без полной перезагрузки страницы)
        /// </summary>
        protected override void RefreshCurrentDoc()
        {
            base.RefreshCurrentDoc();
            RefreshPositions();
        }

        /// <summary>
        ///  Копирование данных документа на контролы. 
        ///  Этот метод может оставатся пустым, если нет необходимости в копировании документа
        /// </summary>
        protected override void DocumentToControls()
        {
            Company.Value = Mtr.Organization.ValueString;
            Subdivision.Value = Mtr.Subdivision.ValueString;
            PerformerOfSubdivision.Value = Mtr.PerformerOfSubdivision.ValueString;
            DBSDocBasis.Filter.PersonIDs.Value = Mtr.Organization.ValueString;

            DBSDocBasis.SelectedItems.AddRange(Mtr.GetDocLinksItems(Mtr.Basis.DocFieldId));
        }

        /// <summary>
        ///  Проверка корректности вводимых полей
        /// </summary>
        /// <returns>true - OK</returns>
        protected override bool ValidateDocument(out List<string> errors, params string[] exeptions)
        {
            base.ValidateDocument(out errors);

            if(Mtr.Positions == null || Mtr.Positions.Count == 0)
                errors.Add("Необходимо добавить хотя бы одну позицию по заявке");

            if (errors.Count > 0)
                return false;

            return true;
        }

        private void ShowAddDialog()
        {
            ClearAddEditDialog();

            // формируем заголовок
            JS.Write("gi('dlgHeaderText').innerText='{0}';", lblAddition);

            // добавление события нажатия на кнопку
            JS.Write("gi('{0}').onclick={1};", btnAction.HtmlID, "function(){cmd('cmd','AddEditPosition');}");

            // Показать диалог
            JS.Write("ShowMtrDlg();");
        }

        /// <summary>
        ///  Показать диалог редактирования
        /// </summary>
        private void ShowEditDialog(int order)
        {
            ShowPositionDetails(order);
            ShowChangeInfo(order);

            // формируем заголовок
            JS.Write("gi('dlgHeaderText').innerText='{0}';", lblEdit);

            // добавление события нажатия на кнопку
            JS.Write("gi('{0}').onclick={1};", btnAction.HtmlID, "function(){cmd('cmd','AddEditPosition','mtrPos', '" + order + "');}");

            // Показать диалог
            JS.Write("ShowMtrDlg();");
        }

        /// <summary>
        ///  Показать диалог копирования
        /// </summary>
        private void ShowCopyDialog(int order)
        {
            ShowPositionDetails(order);

            // формируем заголовок
            JS.Write("gi('dlgHeaderText').innerText='{0}';", cmdCopy);

            // добавление события нажатия на кнопку
            JS.Write("gi('{0}').onclick={1};", btnAction.HtmlID, "function(){cmd('cmd','AddEditPosition');}");

            // Показать диалог
            JS.Write("ShowMtrDlg();");
        }

        /// <summary>
        ///  Показать диалог удаления
        /// </summary>
        private void ShowDeleteDialog(int order)
        {
            var item = Mtr.Positions.First(p => p.MtrOrder == order);
            // формируем строку сообщения
            JS.Write("gi('divDelMessage').innerText='{0}';", string.Format("Вы действительно хотите удалить наименование: {0}?", item.MtrName));

            // добавление события нажатия на кнопку
            JS.Write("gi('{0}').onclick={1};", btnDelete.HtmlID, "function(){cmd('cmd','DeletePosition','mtrPos', '" + order + "');}");

            // Показать диалог
            JS.Write("gi('DeleteDialog').style.display='block';");
        }

        /// <summary>
        ///  Заполнить конкретные значения позиции в окне
        /// </summary>
        public void ShowPositionDetails(int order)
        {
            ClearAddEditDialog();

            var pos = Mtr.Positions.Find(p => p.MtrOrder == order);
            if (pos != null)
            {
                txbMtrName.Value = pos.MtrName;
                txbSpecifications.Value = pos.Specifications;
                txaPurposeOfAcquisition.Value = pos.PurposeOfAcquisition;
                dpPurchasesTerm.ValueDate = pos.PurchasesTerm == DateTime.MinValue ? (DateTime?)null : pos.PurchasesTerm;
                txbUnit.Value = pos.Unit;
                txbQuantity.Value = pos.Quantity == null ? null: pos.Quantity.Value.ToString("0.####");
                txaDescription.Value = pos.Description;
            }
        }

        /// <summary>
        ///  Показать информацию об изменениях
        /// </summary>
        private void ShowChangeInfo(int order)
        {
            var pos = Mtr.Positions.Find(p => p.MtrOrder == order);
            if (pos != null && pos.ChangedDateTime > DateTime.MinValue && pos.UserChangedId > 0)
            {
                chChanged.ChangedByID = pos.UserChangedId;
                chChanged.SetChangeDateTime = pos.ChangedDateTime;
            }
        }

        /// <summary>
        ///  Валидация позиции документа
        /// </summary>
        /// <returns> true - OK</returns>
        private bool ValidateMtrPosition(MTRClaimItem pos)
        {
            var erros = new List<string>();
            if (string.IsNullOrWhiteSpace(pos.MtrName))
                erros.Add("Не заполнено поле: 'Наименование и тип'");

            if (string.IsNullOrWhiteSpace(pos.Specifications))
                erros.Add("Не заполнено поле: 'Технические характеристики'");

            if (string.IsNullOrWhiteSpace(pos.PurposeOfAcquisition))
                erros.Add("Не заполнено поле: 'Цель приобретения'");

            if (pos.PurchasesTerm == DateTime.MinValue)
                erros.Add("Не заполнено поле: 'Срок закупки'");
            else if (pos.PurchasesTerm < DateTime.Today)
                erros.Add("Срок закупки не может быть меньше текущей даты");

            if (string.IsNullOrWhiteSpace(pos.Unit))
                erros.Add("Не заполнено поле: 'Единица измерения'");

            if (pos.Quantity == null)
                erros.Add("Не заполнено поле: 'Количество'");
            else if (pos.Quantity == 0)
                erros.Add("Поле 'Количество' не может принимать значение 0");
            else if(pos.Quantity < 0)
                erros.Add("Поле 'Количество' не может принимать минусовое значение");

            // отключено по просьбе  Анисимова
            //if (pos.MtrPositionId == 0 && IdentityExists(pos))
            //    erros.Add("Существует полностью идентичная запись");

            if (erros.Count == 0)
                return true;

            RenderErrors(erros, "Добавление или изменение позиции невозможно");
            return false;
        }

        /// <summary>
        ///  Проверяет полностью идентичные записи в Positions
        /// </summary>
        private bool IdentityExists(MTRClaimItem pos)
        {
           return Mtr.Positions.Any(p => IsIdentical(p, pos));
        }

        /// <summary>
        ///  проверяет две позиции и возвращает true если они полностью идентичны
        /// </summary>
        /// <returns>true - identical</returns>
        private bool IsIdentical(MTRClaimItem item1, MTRClaimItem item2)
        {
            if (item1.MtrName != item2.MtrName)
                return false;
            if (item1.Specifications != item2.Specifications)
                return false;
            if (item1.PurposeOfAcquisition != item2.PurposeOfAcquisition)
                return false;
            if (item1.PurchasesTerm != item2.PurchasesTerm)
                return false;
            if (item1.Unit != item2.Unit)
                return false;
            if (item1.Quantity != item2.Quantity)
                return false;
            if (item1.Description != item2.Description)
                return false;

            return true;
        }

        /// <summary>
        ///  Добавление позиции
        /// </summary>
        private void AddPosition()
        {
            var newPos = new MTRClaimItem();
            newPos.DocumentId = Doc.DocId;
            newPos.MtrOrder = Mtr.Positions.Count + 1;
            newPos.MtrName = txbMtrName.Value;
            newPos.Specifications = txbSpecifications.Value;
            newPos.PurposeOfAcquisition = txaPurposeOfAcquisition.Value;
            newPos.PurchasesTerm = dpPurchasesTerm.ValueDate ?? DateTime.MinValue;
            newPos.Unit = txbUnit.Value;
            newPos.Description = string.IsNullOrWhiteSpace(txaDescription.Value) ? "-" : txaDescription.Value;
            newPos.Quantity = txbQuantity.ValueDecimal;

            if (!ValidateMtrPosition(newPos))
                return;

            if (!Mtr.IsNew)
            {
                newPos.CreateDeadlockSafety();
                Mtr.Positions.Add(newPos);
            }
            else
            {
                Mtr.Positions.Add(newPos);
                var result = SaveDocument(false);
                if (!result)
                    Mtr.Positions.Remove(newPos);

            }

            JS.Write("cmd('cmd','RefreshPositions');");
            JS.Write("HidePositionDlg();");
        }

        /// <summary>
        ///  Редактирование позиции
        /// </summary>
        private void EditPosition(int id)
        {
            var pos = Mtr.Positions.Find(p => p.MtrOrder == id);
            if (pos != null)
            {
                pos.MtrName = txbMtrName.Value;
                pos.Specifications = txbSpecifications.Value;
                pos.PurposeOfAcquisition = txaPurposeOfAcquisition.Value;
                pos.PurchasesTerm = dpPurchasesTerm.ValueDate ?? DateTime.MinValue;
                pos.Unit = txbUnit.Value;
                pos.Description = string.IsNullOrWhiteSpace(txaDescription.Value) ? "-" : txaDescription.Value;
                pos.Quantity = string.IsNullOrEmpty(txbQuantity.Value) ? 0M : txbQuantity.Value.ToDecimal();

                if (!ValidateMtrPosition(pos))
                    return;

                if (!Mtr.IsNew)
                {
                    pos.UpdateData();
                }
            }

            JS.Write("cmd('cmd','RefreshPositions');");
            JS.Write("HidePositionDlg();");
        }

        /// <summary>
        /// Удаление позиции
        /// </summary>
        private void DeletePosition(int id)
        {
            var index = Mtr.Positions.FindIndex(p => p.MtrOrder == id);
            if (index != -1)
            {
                var toDelete = Mtr.Positions[index];

                if (!Mtr.IsNew)
                {
                    toDelete.Delete();
                }

                Mtr.Positions.RemoveAt(index);

                // пересчет номера в коллекции
                // p.s пересчет номера в базе происходит в тригере
                for (int i = index; i < Mtr.Positions.Count; i++)
                    Mtr.Positions[i].MtrOrder--;
            }

            JS.Write("cmd('cmd','RefreshPositions');");
            JS.Write("gi('DeleteDialog').style.display = 'none';");
        }

        /// <summary>
        ///  Поднять позицию вверх
        /// </summary>
        private void UpperPosition(int order)
        {
            if (order == 1)
                return;

            var index = Mtr.Positions.FindIndex(p => p.MtrOrder == order);
            if (index != -1)
            {
                if (index == 0) return;

                // если новый то маняем в коллекции
                // если из БД, то там своя логика передвижения позиции
                if (Mtr.IsNew)
                    Mtr.Positions[index].MtrOrder--;
                else
                    Mtr.Positions[index].MtrOrder-=2;
                   
                try
                {
                    if (!Mtr.IsNew)
                        Mtr.Positions[index].UpdateRowOrder();
                }
                catch (Exception e)
                {
                    // откатываем значение назад
                    Mtr.Positions[index].MtrOrder++; 

                    var dex = new DetailedException("Ошибка изменения порядка позиции: " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                Mtr.Positions[index - 1].MtrOrder++;

                JS.Write("cmd('cmd','RefreshPositions');");
            }

            RestoreCursor();
        }

        /// <summary>
        ///  Опустить позицию вниз
        /// </summary>
        private void DownPosition(int order)
        {
            if (order == Mtr.Positions.Count)
                return;

            var index = Mtr.Positions.FindIndex(p => p.MtrOrder == order);
            if (index != -1)
            {
                if (index == Mtr.Positions.Count - 1)
                    return;

                Mtr.Positions[index].MtrOrder++;

                try
                {
                    if (!Mtr.IsNew)
                        Mtr.Positions[index].UpdateRowOrder();
                }
                catch (Exception e)
                {
                    // откатываем значение назад
                    Mtr.Positions[index].MtrOrder--;

                    var dex = new DetailedException("Ошибка изменения порядка позиции: " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                Mtr.Positions[index + 1].MtrOrder--;

                JS.Write("cmd('cmd','RefreshPositions');");
            }

            RestoreCursor();
        }


        /// <summary>
        ///  Привести диалог добавления/редактирования в исходное состояние
        /// </summary>
        private void ClearAddEditDialog()
        {
            txbMtrName.Focus();

            chChanged.ChangedByID = null;
            chChanged.Change = null;

            txbMtrName.Value = string.Empty;
            txbSpecifications.Value = string.Empty;
            txaPurposeOfAcquisition.Value = string.Empty;
            dpPurchasesTerm.ValueDate = null;
            txbUnit.Value = string.Empty;
            txbQuantity.Value = string.Empty;
            txaDescription.Value = string.Empty;

            btnAction.OnClick = string.Empty;
        }

        /// <summary>
        ///  Обновить таблицу позиций
        /// </summary>
        public void RefreshPositions(bool reload = true)
        {
            using (var w = new StringWriter())
            {
                if (reload)
                    Mtr.ReloadPositions();

                RenderPositions(w);
                JS.Write("gi('divPositions').innerHTML={0};", HttpUtility.JavaScriptStringEncode(w.ToString(), true));
            }
        }

        /// <summary>
        ///  Отрисовка позиций
        /// </summary>
        public void RenderPositions(TextWriter w)
        {
            w.Write("<table border=1 style='border:1px solid gray; border-collapse:collapse;'>");

            if (Mtr.Positions != null && Mtr.Positions.Count > 0)
                w.Write("<tr class='gridHeader' align='center'>");
            else
                w.Write("<tr align='center'>");

            // рендер кнопки добавления
            if (DocEditable && !IsPrintVersion)
            {
                w.Write("<td><input title='{0}' name='' type='image' alt='' src='/styles/new.gif' border=0 complete='complete' style='cursor:hand'", lblAddition);
                w.Write(" onkeydown=\"if(event.keyCode==13){event.returnValue=false;cmd('cmd','AddEditDialog','mtrPos', '');}\"");
                w.Write(" onclick=\"cmd('cmd','AddEditDialog');\"/></td>");
            }

            w.Write("<td width='20px'>№ п/п</td>");
            w.Write("<td width='150px'>{0}</td>", lblMtrName);
            w.Write("<td width='100px'>{0}</td>", lblSpecifications);
            w.Write("<td width='200px'>{0}</td>", lblPurposeOfAcquisition);
            w.Write("<td width='30px'>{0}</td>", lblPurchasesTerm);
            w.Write("<td width='20px'>{0}</td>", lblUnitShort);
            w.Write("<td width='10px'>{0}</td>", lblPosCol2);
            w.Write("<td width='150px'>{0}</td>", lblNote);

            w.Write("");

            if (DocSigned)
            {
                var tooltip = Resx.GetString("ttAddShetDog");
                w.Write("<td class=\"clHover brdTop brdRight\" style=\"width:150px; cursor: pointer;\" title=\"" + tooltip + "\" onclick=\"ShowLinksDialog();\" cursor: pointer>{0}{1}</td>", "<img style=\"border-image: none; float: left; text-align: left;\" src=\"/styles/OutDoc.gif\">", "Счет, договор");
                w.Write("<td class=\"brdTop\" width='150px''>{0}</td>", "Платежи");
                w.Write("<td class=\"brdTop brdLeft\" width='150px''>{0}</td>", "ТТН");
            }

            w.Write("</tr>");

            if (Mtr.Positions != null)
            {
                Mtr.Positions.Sort((a, b) => a.MtrOrder.CompareTo(b.MtrOrder));
                int counter = 0;
                foreach (var pos in Mtr.Positions)
                {
                    counter++;
                    w.Write("<tr onMouseover=\"this.bgColor='LightGrey'\" onMouseout=\"this.bgColor='whitesmoke'\" ondragover=\"allowDrop();\"  ondrop=\"GetDropInfo('{0}');\">", pos.MtrOrder);

                    // рендер кнопки редактирования
                    if (DocEditable && !IsPrintVersion)
                    {
                        w.Write("<td nowrap>");
                        // рендер кнопки редактирования
                        w.Write("<input title='{0}' name='' type='image' alt='' src='/styles/edit.gif' border=0 complete='complete' style='cursor:hand'", lblEdit);
                        w.Write(" onclick=\"cmd('cmd','AddEditDialog','mtrPos', '" + pos.MtrOrder + "');\"/>");

                        w.Write("&nbsp;");

                        // рендер кнопки копирования
                        w.Write("<input title='{0}' name='' type='image' alt='' src='/styles/Copy.gif' border=0 complete='complete' style='cursor:hand'", cmdCopy);
                        w.Write(" onclick=\"cmd('cmd','CopyDialog','mtrPos', '" + pos.MtrOrder + "');\"/>");

                        w.Write("&nbsp;");
                        // рендер кнопки удаления + позиционирование диалога под кнопкой
                        w.Write("<input title='{0}' name='' type='image' alt='' src='/styles/Delete.gif' border=0 complete='complete' style='cursor:hand'", lblRemoval);
                        w.Write(" onclick=\"cmd('cmd','DeleteDialog','mtrPos', '" + pos.MtrOrder + "'); var near = $('#DeleteDialog');  near.css({top:event.pageY + 10, left: event.pageX + 10});\"/>");

                        w.Write("</td>");
                    }

                    // номер
                    w.Write("<td align='center'>");
                    w.Write(pos.MtrOrder);
                    w.Write("</td>");

                    // Наименование и тип
                    w.Write("<td>");
                    w.Write(pos.MtrName);
                    w.Write("</td>");

                    // Технические характеристики
                    w.Write("<td>");
                    w.Write(pos.Specifications);
                    w.Write("</td>");

                    // Цель приобретения
                    w.Write("<td>");
                    w.Write(pos.PurposeOfAcquisition);
                    w.Write("</td>");

                    // Сроки закупки
                    w.Write("<td>");
                    w.Write(pos.PurchasesTerm.ToString("dd.MM.yyyy"));
                    w.Write("</td>");

                    // Ед. Измерения
                    w.Write("<td align='center'>");
                    w.Write(pos.Unit);
                    w.Write("</td>");

                    // Кол-во
                    w.Write("<td align='center'>");
                    w.Write(pos.Quantity == null ? "" : pos.Quantity.Value.ToString("G"));
                    w.Write("</td>");

                    // Примечание
                    w.Write("<td align='center'>");
                    w.Write(pos.Description);
                    w.Write("</td>");

                    if (DocSigned)
                    {
                        var bottomTr = Mtr.Positions.Count == counter;

                        // Счета, договора
                        w.Write(bottomTr ? "<td class=\"brdBottom brdRight\">" : "<td class=\"brdRight\">");
                        pos.RenderPositionDocLinks(w, Mtr.PositionDocLinks, MtrChildType.ДокументОснованиеПлатежа);
                        w.Write("</td>");

                        // Основания платежа
                        w.Write(bottomTr ? "<td class=\"brdBottom\">" : "<td>");
                        pos.RenderPositionDocLinks(w, Mtr.PositionDocLinks, MtrChildType.ДокументОплаты);
                        w.Write("</td>");

                        // ТТН
                        w.Write(bottomTr ? "<td class=\"brdBottom brdLeft\">" : "<td class=\"brdLeft\">");
                        pos.RenderPositionDocLinks(w, Mtr.PositionDocLinks, MtrChildType.ДокументТТН);
                        w.Write("</td>");
                    }

                    // кнопки изменения позиции
                    if (DocEditable && !IsPrintVersion)
                    {
                        int count = Mtr.Positions.Count;

                        if (count > 1)
                        {
                            int index;
                            if (pos.MtrPositionId == 0)
                                index = Mtr.Positions.FindIndex(i => i.MtrOrder == pos.MtrOrder);
                            else
                                index = Mtr.Positions.FindIndex(i => i.MtrPositionId == pos.MtrPositionId);


                            w.Write("<td>");
                            if (index != 0)
                            {
                                w.Write("<div><input title='передвинуть вверх' name='' type='image'src='/styles/ScrollUpEnabled.gif' style='cursor:hand' ");
                                w.Write("onclick=\"Wait.render(true); cmdasync('cmd','UpPosition','mtrPos', '" + pos.MtrOrder + "');\"/></div>");
                            }

                            if (index != count - 1)
                            {
                                w.Write("<div><input title='передвинуть вниз' name='' type='image' src='/styles/ScrollDownEnabled.gif' style='cursor:hand' ");
                                w.Write("onclick=\"Wait.render(true); cmdasync('cmd','DownPosition','mtrPos', '" + pos.MtrOrder + "');\"/></div>");
                            }
                            w.Write("</td>");
                        }
                    }

                    w.Write("</tr>");
                }
            }

            w.Write("</table>");
        }

        /// <summary>
        ///  Отрисовка позиций для подтверждения
        /// </summary>
        public void RenderPositForDialog(TextWriter w, int checkedPos, int dropDocId)
        {
            if(Mtr.IsNew) return;

            w.Write("<table border=1 style='border:1px solid gray; border-collapse:collapse;'>");

            if (Mtr.Positions != null && Mtr.Positions.Count > 0)
                w.Write("<tr class='gridHeader' align='center'>");
            else
                w.Write("<tr align='center'>");

            // рендер выделить все
            w.Write("<td>");
            var cheAll = new CheckBox
            {
                ID = "cheAll",
                V4Page = this,
                Text = ""
            };
            cheAll.RenderControl(w);
            cheAll.Changed += CheckAll_Changed;
            V4Controls.Add(cheAll);

            w.Write("</td>");

            w.Write("<td width='20px'>№ п/п</td>");
            w.Write("<td width='150px'>{0}</td>", lblMtrName);
            w.Write("<td width='100px'>{0}</td>", lblPartialPay);


            w.Write("</tr>");

            if (Mtr.Positions != null)
            {
                Mtr.ClearAllChecksPositions();
                ClearCheckControls();
                Mtr.Positions.Sort((a, b) => a.MtrOrder.CompareTo(b.MtrOrder));
                foreach (var pos in Mtr.Positions)
                {
                    w.Write("<tr onMouseover=\"this.bgColor='LightGrey'\" onMouseout=\"this.bgColor='whitesmoke'\">");

                    // рендер галки добавления
                    w.Write("<td nowrap>");

                    pos.Checked = pos.MtrOrder == checkedPos;

                    var che = new CheckBox
                    {
                        ID = "che0" + pos.MtrOrder,
                        V4Page = this,
                        Text = "",
                        Checked = pos.Checked
                    };
                    che.RenderControl(w);
                    che.Changed += CheckPosLink_Changed;
                    V4Controls.Add(che);

                    w.Write("</td>");
                   
                    // номер
                    w.Write("<td align='center'>");
                    w.Write(pos.MtrOrder);
                    w.Write("</td>");

                    // Наименование и тип
                    w.Write("<td>");
                    w.Write(pos.MtrName);
                    w.Write("</td>");

                    // частичная оплата
                    w.Write("<td>");
                    w.Write("<input id='chprt0{0}' style='display: none;' type='checkbox' onclick='var el = gi(\"prt0{0}\"); if(this.checked) {{el.style.display = \"inline\";}}" +
                            " else{{el.style.display = \"none\";el.value = \"\";cmd(\"cmd\", \"PartialPay\", \"numVal\", \"0\", \"DropDocId\", \"{1}\");}}'/>&nbsp;"+
                            "<INPUT id = 'prt0{0}' title='Количество' type='number' style='display: none; width: 30px;' min='0' max='{2}'" +
                            " onchange='cmd(\"cmd\", \"PartialPay\", \"numVal\", this.value.toString(), \"DropDocId\", \"{1}\");'/>", pos.MtrOrder, dropDocId, pos.Quantity != null? pos.Quantity.Value.ToString("####"): "");
                    w.Write("</td>");

                    w.Write("</tr>");
                }
            }

            w.Write("</table>");
        }

        /// <summary>
        ///  Обновить таблицы для формы добавления связей 
        /// </summary>
        public void RefreshLinksDialogTables()
        {
            using (var w = new StringWriter())
            {
                RenderLinksDialogTables(w);
                JS.Write("gi('linkPayBasis').innerHTML={0};", HttpUtility.JavaScriptStringEncode(w.ToString(), true));
            }
        }

        /// <summary>
        /// Обновить нераспределенные документы таблицы для формы добавления связей 
        /// </summary>
        public void RefreshNOTDistributedTable()
        {
            using (var w = new StringWriter())
            {
                RenderPayBasisNOTDistributed(w, true);
                JS.Write("gi('NOTDistributedDocs').innerHTML={0};", HttpUtility.JavaScriptStringEncode(w.ToString(), true));
            }
        }

        /// <summary>
        ///  Сформировать таблицы для формы добавления связей
        /// </summary>
        public void RenderLinksDialogTables(TextWriter w)
        {
            if (Mtr.IsNew)
                return;

               payDocsSchetDogovor = MtrChildDoc.GetPayDocumentsNotDistributed(Mtr.DocId);

               RenderPayBasisNOTDistributed(w, false);
               
               w.Write("<br/>");
               w.Write("<br/>");

               RenderPay(w); 
        }

        /// <summary>
        ///  Показ на форме НЕ распределенные документы
        /// </summary>
        public void RenderPayBasisNOTDistributed(TextWriter w, bool refreshFromDb)
        {
            if (Mtr.IsNew)
                return;

            if (refreshFromDb)
                payDocsSchetDogovor = MtrChildDoc.GetPayDocumentsNotDistributed(Mtr.DocId);

            var notDistrPayDocs = NOTDistributedPayDocs;

            RenderDragAndDropTable(w, notDistrPayDocs, false);
        }

        /// <summary>
        ///  Показ на диалоге распределенные документы
        /// </summary>
        public void RenderPay(TextWriter w)
        {
            if (Mtr.IsNew)
                return;

            var distrPayDocs = DistributedPayDocs;

            RenderDragAndDropTable(w, distrPayDocs, true);
        }

        /// <summary>
        ///  Отрисовать документы готовые к DragAndDrop
        /// </summary>
        /// <param name="w">Поток</param>
        /// <param name="payDocs">Список документов</param>
        /// <param name="distributed">Распределенные/не распределенные</param>
        private void RenderDragAndDropTable(TextWriter w, List<MtrChildDoc> payDocs, bool distributed)
        {
            w.Write("<div style='display: block; width: 99.5%;'>");

            if (payDocs.Count > 0)
            {
                w.Write("<div style='display: inline-table; margin-right: 10px;'>");

                if (distributed)
                    w.Write("<div>{0}:</div>", "Распределенные документы");
                else
                    w.Write("<div style='background-color:rgba(190, 116, 116, 1); width: 257px; text-align:center;'>{0}:</div>", "Не распределенные документы");

                w.Write("<table border=1 style='border:1px solid gray; border-collapse:collapse;'>");
                w.Write("<tr align='center'>");


                w.Write("</tr>");

                w.Write("<tr>");
                w.Write("<td align='center' class='gridHeader' width='200px'>{0}</td>", MsgDocs);
                w.Write("<td align='center' class='gridHeader' width='50px'>{0}</td>", lblPosCol10);
                w.Write("</tr>");


                foreach (var s in payDocs)
                {
                    w.Write("<tr onMouseover=\"this.bgColor='LightGrey'\"onMouseout=\"this.bgColor='whitesmoke'\">");

                    // Документ основания оплаты
                    w.Write("<td>");

                    w.Write("<a draggable='true' ondragstart='SetDragInfo({0});' onclick='OpenDoc({0});' href='#'> <img border='0' src='/styles/DocMain.gif'>", s.DocId);
                    w.Write(s.DocumentName);
                    w.Write("</a>");
                    w.Write("</td>");

                    // Сумма основания оплаты
                    w.Write("<td align='right'>");
                    w.Write(s.MoneyString + (IsRusLocal ? s.CurrencyRur : s.CurrencyEng));
                    w.Write("</td>");

                    w.Write("</tr>");
                }

                w.Write("</table>");
                w.Write("</div>");
            }

            w.Write("</div>");
        }

        /// <summary>
        ///  Обработка перетаскивания документа
        /// </summary>
        private void DocDropedAction(int pos, int dropDocId)
        {

            var mtrPos = Mtr.Positions.First(p => p.MtrOrder == pos);

            if(Mtr.PositionDocLinks.Exists(i => i.DocId == dropDocId && i.LinkType == MtrChildType.ДокументОснованиеПлатежа && i.MtrPositionId == mtrPos.MtrPositionId))
            {
                ShowMessage("Для данной позиции уже связан аналогичный документ");
                return;
            }

            using (var w = new StringWriter())
            {
                RenderPositForDialog(w, pos, dropDocId);
                JS.Write("gi('CheckedPositions').innerHTML={0};", HttpUtility.JavaScriptStringEncode(w.ToString(), true));
            }

            // позиционировать диалог там, где претащили
            JS.Write("var near = $('#LinkConfirm');  near.css({top:event.pageY + 10, left: event.pageX + 10});");

            //показать диалог
            JS.Write("gi('LinkConfirm').style.display='block';");

            // показать в выбраной строке частичную оплату
            JS.Write("var chprt = gi(\"chprt0{0}\"); chprt.style.display = \"inline\";", pos);

            // добавление события нажатия на кнопку
            JS.Write("gi('{0}').onclick={1};", btnConfLnk.HtmlID, "function(){cmd('cmd','DocLinked','DropDocId', '" + dropDocId + "'); HideLinkConfirmDlg();}");
        }

        /// <summary>
        /// Не распределенные документы
        /// </summary>
        private List<MtrChildDoc> NOTDistributedPayDocs
        {
            get
            {
                if (payDocsSchetDogovor != null)
                {
                    var notDistr = new List<MtrChildDoc>();
                    foreach (var pd in payDocsSchetDogovor)
                    {
                        if(!Mtr.PositionDocLinks.Exists(i=> i.DocId == pd.DocId && i.LinkType == MtrChildType.ДокументОснованиеПлатежа))
                            notDistr.Add(pd);
                    }

                    return notDistr;
                }

                return new List<MtrChildDoc>();
            }
        }

        /// <summary>
        ///  Распределенные документы
        /// </summary>
        private List<MtrChildDoc> DistributedPayDocs
        {
            get
            {
                if (payDocsSchetDogovor != null)
                {
                    var notDistr = new List<MtrChildDoc>();
                    foreach (var pd in payDocsSchetDogovor)
                    {
                        if (Mtr.PositionDocLinks.Exists(i => i.DocId == pd.DocId && i.LinkType == MtrChildType.ДокументОснованиеПлатежа))
                            notDistr.Add(pd);
                    }

                    return notDistr;
                }

                return new List<MtrChildDoc>();
            }
        }

        /// <summary>
        ///  Документы основание оплаты - счет, договор, приложение к договору
        /// </summary>
        private List<MtrChildDoc> payDocsSchetDogovor;

        /// <summary>
        ///  Добавление связи к выбранным позициям
        /// </summary>
        private void DocLinkedAction(int ldocId)
        {
            if (Mtr.Positions.Exists(i => i.Checked))
            {
                var doc = payDocsSchetDogovor.First(i => i.DocId == ldocId);

                // для одного документа, один набор вытекающих документов,
                // чтобы не делать кучу однообразных запросов один раз получаем, а потом клонируем
                var linkDocs = MtrChildDoc.GetLinkedDocs(ldocId);

                foreach (var p in Mtr.Positions)
                {
                    if (p.Checked)
                    {
                        // не добавлять документ, если он уже есть
                        if (!Mtr.PositionDocLinks.Exists(i => i.DocId == ldocId && i.MtrPositionId == p.MtrPositionId))
                        {
                            var linkDoc = new MtrChildDoc
                            {
                                DocId = ldocId,
                                DocumentName = doc.DocumentName,
                                LinkType = MtrChildType.ДокументОснованиеПлатежа,
                                MtrPositionId = p.MtrPositionId
                            };

                            if (doc.PartialQuantity == 0 || doc.PartialQuantity == p.Quantity)
                                linkDoc.PartialQuantity = p.Quantity.Value;
                            //else if (doc.PartialQuantity > p.Quantity)
                            //    linkDoc.PartialQuantity = p.Quantity.Value;
                            else
                                linkDoc.PartialQuantity = doc.PartialQuantity;

                            linkDoc.Create();

                            Mtr.PositionDocLinks.Add(linkDoc);

                            var lnkDocs = MtrChildDoc.CloneCollection(linkDocs);
                            foreach (var l in lnkDocs)
                            {
                                l.MtrPositionId = p.MtrPositionId;
                                l.ParentId = ldocId;
                            }

                            Mtr.PositionDocLinks.AddRange(lnkDocs);
                        }
                    }
                }

                RefreshPositions(false);
                RefreshNOTDistributedTable();
                RefreshLinksDialogTables();
            }
        }

        /// <summary>
        ///  Удаление связаного документа
        /// </summary>
        private void LinkedDocDelete(int posId, int lnkDocId, bool ask)
        {
            if (ask)
            {
                JS.Write("CustomConfirmChangedTwoButtons.save = function() {SetItemParam('ConfirmChangedTwoButtons');" +
                         "gi('v4_divDialogBox').style.display = \"none\";" +
                         "gi('v4_divDialogOverlay').style.display = \"none\";" +
                         "cmd('cmd', 'RemoveLinkDoc','mtrPos', '" + posId + "', 'LnkDocId', '" + lnkDocId + "', 'ask', '0');};");

                var doc = payDocsSchetDogovor.FirstOrDefault(i => i.DocId == lnkDocId);

                var msg = Resx.GetString("msgQuestionDelete");
                var confirmRemoveMsg = string.Format(msg , doc != null? doc.DocumentName: "");

                JS.Write("CustomConfirmChangedTwoButtons.render('Удаление', '{0}', '{1}', '{2}', '');", confirmRemoveMsg, Resx.GetString("btnDelete"), Resx.GetString("ppBtnCancel"));
 
                return;
            }

            var index = Mtr.Positions.FindIndex(p => p.MtrPositionId == posId);
            if (index != -1)
            {
                var pos = Mtr.Positions[index];
                foreach (var p in Mtr.PositionDocLinks)
                {
                    if (p.LinkType == MtrChildType.ДокументОснованиеПлатежа && p.MtrPositionId == pos.MtrPositionId && p.DocId == lnkDocId)
                    {
                        p.Delete();
                    }
                }

                Mtr.PositionDocLinks.RemoveAll(i => i.MtrPositionId == pos.MtrPositionId && (i.DocId == lnkDocId || i.ParentId == lnkDocId));
       
                RefreshPositions(false);
                RefreshLinksDialogTables();
                RefreshNOTDistributedTable();
            }
        }

        #region События от контролов

        /// <summary>
        ///  Установить предупреждение отсутствия/присутствия подписи руководителя
        /// </summary>
        private void SetHeadDivisionText()
        {
            string inscription = string.Empty;
            string subDivision = Mtr.Subdivision.ValueString;
            int orgId = Mtr.Organization.ValueInt;
            if (!string.IsNullOrEmpty(subDivision) && orgId > 0)
            {
                int emplId = MTRClaim.GetHeadDivision(orgId, subDivision);

                if (emplId == 0)
                {
                    inscription = Resx.GetString("msgHeadSignUnable") ?? "";
                }
                // если в списке подписей нет подписи руководителя, 
                // s.SignId > 0 - строка "составил:" не считается, проверять только подписи 
                else if (Doc.DocSigns == null || !Doc.DocSigns.Exists(s => s.SignId > 0 && (s.EmployeeId == emplId || s.EmployeeInsteadOf == emplId)))
                {
                    var userEditUrl = Config.user_form;
                    var headerEmpl = new Employee(emplId.ToString());

                    var linkbtn =
                        string.Format(
                            "<a class='v4_callerControl' data-id='{1}' caller-type='2' style=\"color:red\" href=\"#\" onclick=\"window.open('{0}?id={1}', '_blank', 'location=no, menubar=no, status=no, toolbar=no, resizable=yes, scrollbars=yes');\">{2}</a>",
                            userEditUrl, emplId, IsRusLocal? headerEmpl.FIO : headerEmpl.FIOEn);


                    inscription = Resx.GetString("msgNoHeadSign") + "</br>(" + linkbtn + ")";
                }
            }

            JS.Write("gi('headDivision').innerHTML={0};", HttpUtility.JavaScriptStringEncode(inscription, true));
        }


        /// <summary>
        /// Событие изменения организации
        /// </summary>
        protected void CompanyChanged(object sender, ProperyChangedEventArgs e)
        {
            var selCompany = ((Select)sender).Value;
            Subdivision.Filter.PcId.Value = selCompany;
            Subdivision.Filter.PcId.CompanyHowSearch = "0";
            DBSDocBasis.Filter.PersonIDs.Value = selCompany;
            Mtr.Organization.Value = selCompany.ToInt();

            if (e.OldValue != e.NewValue)
            {
                Subdivision.Value = string.Empty;
                Mtr.Subdivision.Value = string.Empty;

                PerformerOfSubdivision.Value = string.Empty;
                Mtr.PerformerOfSubdivision.Value = 0;
            }
        }

        /// <summary>
        /// Событие изменения подразделения
        /// </summary>
        protected void SubdivisionChanged(object sender, ProperyChangedEventArgs e)
        {
            var select = (Select) sender;
            PerformerOfSubdivision.Filter.SubdivisionIDs.Value = select.SelectedItemsString;
            PerformerOfSubdivision.Filter.SubdivisionIDs.SubdivisionHowSearch = select.ValueSelectEnum;
            PerformerOfSubdivision.Filter.IdsCompany.CompanyHowSearch = "0";
            Mtr.Subdivision.Value = select.Value;

            if (e.OldValue != e.NewValue)
            {
                PerformerOfSubdivision.Value = string.Empty;
                Mtr.PerformerOfSubdivision.Value = 0;
            }

            SetHeadDivisionText();
        }

        /// <summary>
        /// Событие изменения исполнителя
        /// </summary>
        protected void PerformerChanged(object sender, ProperyChangedEventArgs e)
        {
            var curSender = (Select)sender;
            Mtr.PerformerOfSubdivision.Value = curSender.Value.ToInt();
        }

        /// <summary>
        ///  Событие поска исполнителя
        /// </summary>
        protected void PerformerOfSubdivision_OnBeforeSearch(object sender)
        {
            PerformerOfSubdivision.Filter.IdsCompany.Value = Company.Value;
            PerformerOfSubdivision.Filter.IdsCompany.CompanyHowSearch = "0";
            PerformerOfSubdivision.Filter.SubdivisionIDs.Value = string.IsNullOrEmpty(Subdivision.Value) ? "" : "'" + Subdivision.Value + "'";
            PerformerOfSubdivision.Filter.SubdivisionIDs.SubdivisionHowSearch = "0";
            PerformerOfSubdivision.Filter.Status.ValueStatus = СотоянияСотрудника.Работающие;
        }

        /// <summary>
        /// Событие изменения основания документа
        /// </summary>
        protected void DBSDocBasis_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (!e.NewValue.IsNullEmptyOrZero())
            {
                var value = e.NewValue.ToInt();

                if (Mtr.BasisDocLinks.Exists(i => i.BaseDocId == value))
                    return;

                var link = new DocLink {BaseDocId = value, SequelDocId = Doc.DocId, DocFieldId = Mtr.Basis.DocFieldId};

                Mtr.BasisDocLinks.Add(link);
            }
        }

        /// <summary>
        /// Событие удаление значения из списка документа основания
        /// </summary>
        protected void DBSDocBasis_OnDeleted(object sender, ProperyDeletedEventArgs e)
        {
            if (!e.DelValue.IsNullEmptyOrZero())
            {
                var index = Mtr.BasisDocLinks.FindIndex(i => i.BaseDocId == e.DelValue.ToInt());
                if (index != -1)
                {
                    var link = Mtr.BasisDocLinks[index];
                    if (link.DocLinkId > 0)
                        link.Delete();

                    Mtr.BasisDocLinks.RemoveAt(index);
                }
            }
        }

        /// <summary>
        ///  Событие поставить галочку на все чекбоксы
        /// </summary>
        protected void CheckAll_Changed(object sender, ProperyChangedEventArgs e)
        {
            var check = ((CheckBox) sender).Checked;

                foreach (var c in V4Controls)
                {
                    if (c.Key.StartsWith("che0"))
                    {
                        var che = (CheckBox) c.Value;
                        che.Checked = check;
                    }
                }

            foreach (var p in Mtr.Positions)
            {
                p.Checked = check;

                // показывает и скрывает checks для частичной оплаты
                if (check)
                    JS.Write("var chprt = gi(\"chprt0{0}\"); chprt.style.display = \"inline\";", p.MtrOrder);
                else
                    JS.Write("var chprt = gi(\"chprt0{0}\"); var prt = gi(\"prt0{0}\"); chprt.style.display = \"none\"; chprt.checked = 0; prt.style.display = \"none\";", p.MtrOrder);
            }

            // вариант ниже НЕ обновляет серверную модель
            // JS.Write(@"var myList = document.getElementsByClassName('ToCheckAll'); 
            //            for (i = 0; i < myList.length; i++) {{
            //               var ch = myList[i].firstElementChild;
            //               ch.checked = {0};
            //            }}", check ? "true" :"false");
        }

        /// <summary>
        /// Снять выделения с CheckBox -ов выбора позиций
        /// </summary>
        private void ClearCheckControls()
        {
            foreach (var c in V4Controls)
            {
                if (c.Key.StartsWith("che0"))
                {
                    var che = (CheckBox)c.Value;
                    che.Value = "0";
                }
            }
        }

        /// <summary>
        ///  Общий обработчик события для чекбоксы выбора ссылок
        /// </summary>
        protected void CheckPosLink_Changed(object sender, ProperyChangedEventArgs e)
        {
            var che = (CheckBox) sender;
            var pos = che.HtmlID.Replace("che0", "");
            var posInt = pos.ToInt();

            var position = Mtr.Positions.FirstOrDefault(i => i.MtrOrder == posInt);

            if (position != null)
                position.Checked = che.Checked;

            // показывает и скрывает checks для частичной оплаты
            if(che.Checked)
                JS.Write("var chprt = gi(\"chprt0{0}\"); chprt.style.display = \"inline\";", posInt);
            else
                JS.Write("var chprt = gi(\"chprt0{0}\"); var prt = gi(\"prt0{0}\"); chprt.style.display = \"none\"; chprt.checked = 0; prt.style.display = \"none\";", posInt);
        }

        #endregion

        /// <summary>
        /// Событие на изменение связи документа
        /// </summary>
        protected void DocToLink_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (!e.NewValue.IsNullEmptyOrZero())
            {
                var value = e.NewValue.ToInt();
                var ctrl = (Select) sender;

               // DBSDocToLink.Value = string.Empty;

                ConfirmDocToLinkWithMtr(true, value, ctrl.ValueText);
            }
        }

        /// <summary>
        ///  Связывание документов, событие поиска
        /// </summary>
        protected void DBSDocToLink_OnBeforeSearch(object sender)
        {
            DBSDocToLink.Filter.PersonIDs.Value = Company.Value;
        }

        /// <summary>
        ///  Подтверждение связи документа и МТР
        /// </summary>
        protected void ConfirmDocToLinkWithMtr(bool ask, int linkDocId = 0, string docName = "")
        {
            if (payDocsSchetDogovor.Exists(p => p.DocId == linkDocId))
            {
                ShowMessage("Данный документ уже связан с заявкой МТР");
                return;
            }

            var docToLink = new Document(linkDocId.ToString(), false);

            if (docToLink.Available && docToLink.Date < Mtr.Date)
            {
                ShowMessage("Дата связываемого документа не может быть меньше даты заявки МТР");
                return;
            }

            if (ask)
            {
                JS.Write("CustomConfirmChangedTwoButtons.save = function() {SetItemParam('ConfirmChangedTwoButtons');" +
                         "gi('v4_divDialogBox').style.display = \"none\";" +
                         "gi('v4_divDialogOverlay').style.display = \"none\";" +
                         "cmd('cmd', 'LinkWithMtr', 'MtrLnkDocId', '" + linkDocId + "');};");

                var confirmLinkDocMsg = Resx.GetString("msgLinkDoc") + " " + docName + "?";

                JS.Write("CustomConfirmChangedTwoButtons.render('Связывание документа', '{0}', '{1}', '{2}', '');", confirmLinkDocMsg, Resx.GetString("cmdSave"), Resx.GetString("ppBtnCancel"));
            }
            else
            {
                if (linkDocId > 0)
                {
                    var link = new DocLink();
                    link.BaseDocId = Mtr.DocId;
                    link.SequelDocId = linkDocId;
                    link.DocFieldId = null;

                    link.Create();

                    MtrChildDoc lnk = new MtrChildDoc();

                    lnk.DocId = linkDocId;
                    lnk.DocumentName = docName;
                    payDocsSchetDogovor.Add(lnk);

                    RefreshLinksDialogTables();
                    RefreshNOTDistributedTable();
                    DBSDocToLink.Value = string.Empty;
                }
            }
        }

        /// <summary>
        ///  Установка значения частичной оплаты
        /// </summary>
        private void PartialPayChanged(int numValue, int dropDocId)
        {
            var doc = payDocsSchetDogovor.First(i => i.DocId == dropDocId);
            doc.PartialQuantity = numValue;
        }

        /// <summary>
        ///  Отобразить остаток на складах
        /// </summary>
        protected void RenderStoreDistributed(TextWriter w)
        {
            if (Mtr.PositionDocLinks != null && Mtr.PositionDocLinks.Exists(p => p.LinkType == MtrChildType.ДокументТТН))
            {

                var colDocs = Mtr.PositionDocLinks.Where(m => m.LinkType == MtrChildType.ДокументТТН)
                                  .Select(m => m.DocId.ToString());
                var docs = Collection2Str(colDocs);

                if (!string.IsNullOrEmpty(docs))
                {
                    var query = string.Format(@"SELECT Движения.КодДвиженияНаСкладе, Движения.ДатаДвижения,  Движения.РесурсРус, Движения.РесурсЛат, Движения.Количество, 
                    (SELECT TOP 1 ISNULL(ЕдиницаРус, ЕдиницаЛат) FROM Справочники.dbo.ЕдиницыИзмерения AS e WITH(NOLOCK) WHERE e.КодЕдиницыИзмерения = Движения.КодЕдиницыИзмерения) ЕдиницаИзмерения,
                    (SELECT TOP 1 Склад FROM Справочники.dbo.vwСклады AS v WITH(NOLOCK) WHERE v.КодСклада = Движения.КодСкладаПолучателя) НазваниеСклада,
                    (SELECT 
                      SUM(
                      CASE WHEN [ТипТранзакции] = 12 THEN  [Количество]
                           WHEN [ТипТранзакции] = 13 THEN  [Количество] *-1
                      END) 
                    FROM [Документы].[dbo].[vwДвиженияНаСкладах] 
                    WHERE (КодСкладаПолучателя = Движения.КодСкладаПолучателя OR КодСкладаОтправителя = Движения.КодСкладаПолучателя) AND КодРесурса = Движения.КодРесурса
                    GROUP BY КодРесурса) ОстатокНаСкладе -- этот запрос самый длительный
                    FROM vwДвиженияНаСкладах Движения
                    WHERE Движения.КодДокумента IN ({0})", docs);

                    var dt = DBManager.GetData(query, Doc.CN);

                    if (dt.Rows.Count > 0)
                    {
                        w.Write("Остаток на складах:");

                        var aktMoveType = new DocType(DocTypeEnum.АктПриемаПередачиПеремещенияСоСкладаНаСклад);

                        var aktMoveUrl = aktMoveType.URL + "?DocId=" + Mtr.Id;

                        var storeBtn = new Button
                        {
                            ID = "storeBtn",
                            V4Page = this,
                            Text = "Выдать  ",
                            Style = "BACKGROUND: buttonface url(/Styles/Popup.gif) no-repeat right center; background-color: lightGray;",
                            OnClick = "window.open('" + aktMoveUrl +  "','_blank','status=no,toolbar=no,menubar=no,location=no,resizable=yes,scrollbars=yes');"
                        };
                        storeBtn.RenderControl(w);
                        V4Controls.Add(storeBtn);

                        w.Write("<table border=1 style='border:1px solid gray; border-collapse:collapse;'>");
                        w.Write("<tr class='gridHeader' align='center'>");


                        w.Write("<td width='100px' align='center'>Склад</td>");
                        w.Write("<td width='200px' align='center'>{0}</td>", "Товар");
                        w.Write("<td width='60px' align='center'>{0}</td>", "Кол-во по документу");
                        w.Write("<td width='50px' align='center'>{0}</td>", "Остаток");
                        w.Write("<td width='20px' align='center'>{0}</td>", "Ед");

                        w.Write("</tr>");

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            w.Write("<tr>");
                            w.Write("<td align='center'>{0}</td>", dt.Rows[i]["НазваниеСклада"]);
                            w.Write("<td align='center'>{0}</td>", dt.Rows[i]["РесурсРус"]);
                            w.Write("<td align='center'>{0}</td>", dt.Rows[i]["Количество"]);
                            w.Write("<td align='center'>{0}</td>", dt.Rows[i]["ОстатокНаСкладе"]);
                            w.Write("<td align='center'>{0}</td>", dt.Rows[i]["ЕдиницаИзмерения"]);

                            w.Write("</tr>");
                        }

                        w.Write("</table>");

                        // если на складах нет, то и смысла нет смотреть выданные
                        AktIssued(w);
                    }
                }
            }
        }

        /// <summary>
        ///  Выдано по акту
        /// </summary>
        public void AktIssued(TextWriter w)
        {
            var query = string.Format(@"SELECT движения.КодДокумента, движения.РесурсРус, движения.Количество,
(SELECT TOP 1 ЕдиницаРус FROM Справочники.dbo.ЕдиницыИзмерения AS ед WHERE ед.КодЕдиницыИзмерения = Движения.КодЕдиницыИзмерения) ЕдиницаИзмерения
 FROM vwСвязиДокументов связи 
INNER JOIN vwДвиженияНаСкладах движения
ON связи.КодДокументаВытекающего = движения.КодДокумента
WHERE связи.КодДокументаОснования = {0} AND связи.КодПоляДокумента = 1804", Mtr.Id);

            var dt = DBManager.GetData(query, Doc.CN);

            if (dt.Rows.Count > 0)
            {
                w.Write("<br/>");
                w.Write("<div>Выдано:</div>");

                w.Write("<table border=1 style='border:1px solid gray; border-collapse:collapse;'>");
                w.Write("<tr class='gridHeader' align='center'>");


                w.Write("<td width='130px' align='center'>{0}</td>", "Документ");
                w.Write("<td width='200px' align='center'>{0}</td>", "Товар");
                w.Write("<td width='50px' align='center'>{0}</td>", "Количество");
                w.Write("<td width='20px' align='center'>{0}</td>", "Ед");

                w.Write("</tr>");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    w.Write("<tr>");
                    w.Write("<td align='center'><a href='javascript:OpenDoc({0});'> <img src='/styles/DocMain.gif'> Акт перемещения</a></td>", dt.Rows[i]["КодДокумента"]);      
                    w.Write("<td align='center'>{0}</td>", dt.Rows[i]["РесурсРус"]);
                    w.Write("<td align='center'>{0}</td>", dt.Rows[i]["Количество"]);
                    w.Write("<td align='center'>{0}</td>", dt.Rows[i]["ЕдиницаИзмерения"]);

                    w.Write("</tr>");
                }

                w.Write("</table>");
            }
        }

        /// <summary>
        ///  Обновить остаток на складах
        /// </summary>
        public void RefreshStoreDistributed()
        {
            using (var w = new StringWriter())
            {
                RenderStoreDistributed(w);
                JS.Write("gi('StoreDistributed').innerHTML={0};", HttpUtility.JavaScriptStringEncode(w.ToString(), true));
            }
        }
    }
}