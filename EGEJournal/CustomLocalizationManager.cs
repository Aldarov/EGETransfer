using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace EGEJournal
{
    public class CustomLocalizationManager : LocalizationManager
    {
        public override string GetStringOverride(string key)
        {
            switch (key)
            {
                case "GridViewGroupPanelText":
                    return "Перетащите заголовок столбца и отпустите его здесь, чтобы сгруппировать по данному столбцу.";
                //---------------------- RadGridView Filter Dropdown items texts:
                case "GridViewClearFilter":
                    return "Очистить";
                case "GridViewFilterShowRowsWithValueThat":
                    return "Показать строки, которые";
                case "GridViewFilterSelectAll":
                    return "Выбрать все";
                case "GridViewFilterContains":
                    return "содержат";
                case "GridViewFilterDoesNotContain":
                    return "не содержат";
                case "GridViewFilterStartsWith":
                    return "начинаются с";
                case "GridViewFilterEndsWith":
                    return "заканчиваются на";
                case "GridViewFilterIsContainedIn":
                    return "содержатся в";
                case "GridViewFilterIsNotContainedIn":
                    return "не содержатся в";
                case "GridViewFilterIsEqualTo":
                    return "равны";
                case "GridViewFilterIsNotEqualTo":
                    return "не равны";
                case "GridViewFilterIsGreaterThan":
                    return "больше чем";
                case "GridViewFilterIsGreaterThanOrEqualTo":
                    return "больше или равны";
                case "GridViewFilterIsLessThan":
                    return "меньше чем";
                case "GridViewFilterIsLessThanOrEqualTo":
                    return "меньше или равны";
                case "GridViewFilterAnd":
                    return "и";
                case "GridViewFilterOr":
                    return "или";
                case "GridViewFilterMatchCase":
                    return "учитывать регистр";
                case "GridViewFilter":
                    return "Отфильтровать";
                case "GridViewGroupPanelTopTextGrouped":
                    return "Сгруппировано по:";
                case "GridViewGroupPanelTopText":
                    return "Заголовок группы";
                case "GridViewAlwaysVisibleNewRow":
                    return "Нажмите здесь чтобы добавить новую запись";

            }

            return base.GetStringOverride(key);
        }
    }
}
