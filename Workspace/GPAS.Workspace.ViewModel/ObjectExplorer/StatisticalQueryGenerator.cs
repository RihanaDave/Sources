using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.StatisticalQuery.Formula.SetAlgebra;
using GPAS.StatisticalQuery.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet.StartingObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.ViewModel.ObjectExplorer
{
    public class StatisticalQueryGenerator
    {
        List<FormulaStep> formulaSequence;

        public StatisticalQueryGenerator()
        {
            formulaSequence = new List<FormulaStep>();
        }

        public void AppendEquivalentFormula(ObjectSetBase objectSet)
        {
            if (objectSet is StartingObjectSetBase)
            {
                return;
            }
            else if (objectSet is DerivedObjectSet)
            {
                var derivedObjectSet = (objectSet as DerivedObjectSet);

                AppendEquivalentFormula(derivedObjectSet.ParentSet);
                if (derivedObjectSet.AppliedFormula is ViewModel.ObjectExplorer.Formula.TypeBasedDrillDown)
                {
                    AppendEquivalentFormula((derivedObjectSet.AppliedFormula as ViewModel.ObjectExplorer.Formula.TypeBasedDrillDown).FilteredBy);
                }
                else if (derivedObjectSet.AppliedFormula is ViewModel.ObjectExplorer.Formula.PropertyValueBasedDrillDown)
                {
                    AppendEquivalentFormula((derivedObjectSet.AppliedFormula as ViewModel.ObjectExplorer.Formula.PropertyValueBasedDrillDown).FilteredBy);
                }
                else if (derivedObjectSet.AppliedFormula is ViewModel.ObjectExplorer.Formula.PerformSetOperation)
                {
                    AppendEquivalentFormula(derivedObjectSet.AppliedFormula as ViewModel.ObjectExplorer.Formula.PerformSetOperation);
                }
                else if (derivedObjectSet.AppliedFormula is ViewModel.ObjectExplorer.Formula.PropertyValueRangeDrillDown)
                {
                    AppendEquivalentFormula(derivedObjectSet.AppliedFormula as ViewModel.ObjectExplorer.Formula.PropertyValueRangeDrillDown);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void AppendEquivalentFormula(ViewModel.ObjectExplorer.Formula.PerformSetOperation performSetOperation)
        {
            StatisticalQueryGenerator tempGenerator = new StatisticalQueryGenerator();
            tempGenerator.AppendEquivalentFormula(performSetOperation.JoinedSet);
            List<FormulaStep> joinedSetFormulaSeq = tempGenerator.formulaSequence;
            var result = new PerformSetOperation()
            {
                JoinedSetStartPoint = StartingObjectSet.AllObjects,
                JoinedSetFormulaSequence = joinedSetFormulaSeq,
                Operator = ConvertSetAlgebraOperator(performSetOperation.Operator)
            };

            formulaSequence.Add(result);
        }
        private Operator ConvertSetAlgebraOperator(Formula.SetAlgebraOperator setOperator)
        {
            switch (setOperator)
            {
                case Formula.SetAlgebraOperator.Union:
                    return Operator.Union;
                case Formula.SetAlgebraOperator.Intersection:
                    return Operator.Intersection;
                case Formula.SetAlgebraOperator.SubtractRight:
                    return Operator.SubtractRight;
                case Formula.SetAlgebraOperator.SubtractLeft:
                    return Operator.SubtractLeft;
                case Formula.SetAlgebraOperator.ExclusiveOr:
                    return Operator.ExclusiveOr;
                default:
                    throw new NotSupportedException();
            }
        }

        public void AppendEquivalentFormula(IEnumerable<PropertyValueStatistic> filter)
        {
            formulaSequence.Add(GetRelatedFormula(filter));
        }
        private FormulaStep GetRelatedFormula(IEnumerable<PropertyValueStatistic> filter)
        {
            var result = new PropertyValueBasedDrillDown();
            result.Portions = new List<HasPropertyWithTypeAndValue>();
            foreach (PropertyValueStatistic currentFilter in filter)
            {
                HasPropertyWithTypeAndValue newPortion = new HasPropertyWithTypeAndValue()
                {
                    PropertyTypeUri = currentFilter.Category.TypeUri,
                    PropertyValue = currentFilter.PropertyValue
                };
                result.Portions.Add(newPortion);
            }
            return result;
        }

        public void AppendEquivalentFormula(Formula.PropertyValueRangeDrillDown propertyValueRangeFilters)
        {
            var drillDown = new PropertyValueRangeDrillDown()
            {
                DrillDownDetails = GetRelatedFormula(propertyValueRangeFilters)
            };
            formulaSequence.Add(drillDown);
        }

        private PropertyValueRangeStatistics GetRelatedFormula(Formula.PropertyValueRangeDrillDown propertyValueRangeFilters)
        {
            return new PropertyValueRangeStatistics()
            {
                NumericPropertyTypeUri = propertyValueRangeFilters.PropertyTypeUri,
                Bars = propertyValueRangeFilters.ValueRanges.Select(vr => new PropertyValueRangeStatistic() { Start = vr.Start, End = vr.End }).ToList()
            };
        }

        public void AppendEquivalentFormula(IEnumerable<PreviewStatistic> filter)
        {            
            if (filter.Where(f => (f.SuperCategory.Equals(PreviewStatistic.ObjectTypesSuperCategoryTitle) ||
             f.SuperCategory.Equals(PreviewStatistic.PropertyTypesSuperCategoryTitle))).Any())
            {
                formulaSequence.Add(GetRelatedFormula(filter));
            }
            else if (filter.Where(f => (f.SuperCategory.Equals(PreviewStatistic.LinkTypesSuperCategoryTitle) ||
             f.SuperCategory.Equals(PreviewStatistic.LinkedObjectTypesSuperCategoryTitle))).Any())
            {
                formulaSequence.Add(GetRelatedLinkBasedFormula(filter));
            }

        }
        private FormulaStep GetRelatedFormula(IEnumerable<PreviewStatistic> filter)
        {
            var formulaPortionTypeBases = new List<TypeBasedDrillDownPortionBase>();
            foreach (PreviewStatistic currentFilter in filter)
            {
                if (currentFilter.SuperCategory.Equals(PreviewStatistic.ObjectTypesSuperCategoryTitle))
                {
                    formulaPortionTypeBases.Add(new OfObjectType()
                    {
                        ObjectTypeUri = currentFilter.TypeURI
                    });
                }
                else if (currentFilter.SuperCategory.Equals(PreviewStatistic.PropertyTypesSuperCategoryTitle))
                {
                    formulaPortionTypeBases.Add(new HasPropertyWithType()
                    {
                        PropertyTypeUri = currentFilter.TypeURI
                    });
                }
            }
            var result = new TypeBasedDrillDown();
            result.Portions = formulaPortionTypeBases;
            return result;
        }

        private FormulaStep GetRelatedLinkBasedFormula(IEnumerable<PreviewStatistic> filter)
        {
            var formulaPortionLinkBases = new List<LinkBasedDrillDownPortionBase>();
            foreach (PreviewStatistic currentFilter in filter)
            {
                
                if (currentFilter.SuperCategory.Equals(PreviewStatistic.LinkTypesSuperCategoryTitle))
                {
                    formulaPortionLinkBases.Add(new LinkTypeBasedDrillDown()
                    {
                        LinkTypeUri = currentFilter.TypeURI
                    });
                }

                else if (currentFilter.SuperCategory.Equals(PreviewStatistic.LinkedObjectTypesSuperCategoryTitle))
                {
                    formulaPortionLinkBases.Add(new LinkedObjectTypeBasedDrillDown()
                    {
                        LinkedObjectTypeUri = currentFilter.TypeURI
                    });
                }
            }
            var result = new LinkBasedDrillDown();
            result.Portions = formulaPortionLinkBases;
            return result;
        }

        public Query GenerateQuery()
        {
            Query query = new Query()
            {
                SourceSet = StatisticalQuery.ObjectSet.StartingObjectSet.AllObjects,
                FormulaSequence = formulaSequence
            };
            return query;
        }
    }
}
