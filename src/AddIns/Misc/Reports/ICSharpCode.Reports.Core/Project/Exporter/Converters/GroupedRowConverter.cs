﻿/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Peter
 * Datum: 02.01.2009
 * Zeit: 17:33
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using ICSharpCode.Reports.Core.BaseClasses.Printing;
using ICSharpCode.Reports.Core.Interfaces;

namespace ICSharpCode.Reports.Core.Exporter
{
	/// <summary>
	/// Description of RowConverter.
	/// </summary>
	/// 
	
	public class GroupedRowConverter:BaseConverter
	{

		private BaseReportItem parent;
		
		public GroupedRowConverter(IDataNavigator dataNavigator,
		                    ExporterPage singlePage, ILayouter layouter):base(dataNavigator,singlePage,layouter)
		{               
		}
		
		public override ExporterCollection Convert(BaseReportItem parent, BaseReportItem item)
		{
			if (parent == null) {
				throw new ArgumentNullException("parent");
			}
			if (item == null) {
				throw new ArgumentNullException("item");
			}
			ISimpleContainer simpleContainer = item as ISimpleContainer;
			this.parent = parent;
			
			simpleContainer.Parent = parent;
			
			PrintHelper.AdjustParent(parent,simpleContainer.Items);
			if (PrintHelper.IsTextOnlyRow(simpleContainer)) {
				ExporterCollection myList = new ExporterCollection();

				BaseConverter.BaseConvert (myList,simpleContainer,parent.Location.X,
				                  new Point(base.SectionBounds.DetailStart.X,base.SectionBounds.DetailStart.Y));
				
				return myList;
			} else {
				return this.ConvertDataRow(simpleContainer);
			}
		}
		
		private ExporterCollection ConvertDataRow (ISimpleContainer simpleContainer)
		{
			ExporterCollection mylist = new ExporterCollection();
			Point currentPosition = new Point(base.SectionBounds.DetailStart.X,base.SectionBounds.DetailStart.Y);
			BaseSection section = parent as BaseSection;
			
			int defaultLeftPos = parent.Location.X;

			do {
				
				PrintHelper.AdjustSectionLocation (section);
				section.Size = this.SectionBounds.DetailSectionRectangle.Size;
				base.SaveSize(section.Items[0].Size);
				
				// Grouping Header
				
				if (section.Items.HasGroupColumns) {
					currentPosition = ConvertGroupHeader(mylist,section,simpleContainer,defaultLeftPos,currentPosition);
				}
				else
				{
					// No Grouping
					currentPosition = ConvertStandardRow (mylist,section,simpleContainer,defaultLeftPos,currentPosition);
				}
				
				
			// Group Children  ----------------------
			
				if (base.DataNavigator.HasChildren) {
					
					StandardPrinter.AdjustBackColor(simpleContainer,GlobalValues.DefaultBackColor);
					base.DataNavigator.SwitchGroup();
					do {
						currentPosition = ConvertGroupChilds (mylist,section,simpleContainer,defaultLeftPos,currentPosition);
					}
					while ( base.DataNavigator.ChildMoveNext());
				}
			
				// end grouping -----------------
				
				if (PrintHelper.IsPageFull(new Rectangle(new Point (simpleContainer.Location.X,currentPosition.Y), section.Size),base.SectionBounds)) {
					base.FirePageFull(mylist);
					section.SectionOffset = base.SinglePage.SectionBounds.PageHeaderRectangle.Location.Y;
					currentPosition = new Point(base.SectionBounds.PageHeaderRectangle.X,base.SectionBounds.PageHeaderRectangle.Y);
					mylist.Clear();
				}
				
				ShouldDrawBorder (section,mylist);
				
			}
			while (base.DataNavigator.MoveNext());
			
			SectionBounds.ReportFooterRectangle =  new Rectangle(SectionBounds.ReportFooterRectangle.Left,
			                                                     section.Location.Y + section.Size.Height,
			                                                     SectionBounds.ReportFooterRectangle.Width,
			                                                     SectionBounds.ReportFooterRectangle.Height);
			return mylist;
		}
		
		
		
		
		private void AfterConverting (BaseSection section)
		{
			section.Items[0].Size = base.RestoreSize;
			section.SectionOffset += section.Size.Height + 3 * GlobalValues.GapBetweenContainer;
		}
		
		
		
		private Point ConvertGroupHeader(ExporterCollection mylist,BaseSection section,ISimpleContainer simpleContainer,int leftPos,Point offset)
		{
			var groupCollection = section.Items.ExtractGroupedColumns();
			base.DataNavigator.Fill(groupCollection);
			base.FireSectionRendering(section);
			StandardPrinter.EvaluateRow(base.Evaluator,mylist);
			ExporterCollection list = StandardPrinter.ConvertPlainCollection(groupCollection,offset);
			mylist.AddRange(list);
			AfterConverting (section);
			return new Point (leftPos,offset.Y + groupCollection[0].Size.Height + 20  + (3 *GlobalValues.GapBetweenContainer));
		}
		 
		
		private Point ConvertGroupChilds(ExporterCollection mylist, BaseSection section, ISimpleContainer simpleContainer, int defaultLeftPos, Point currentPosition)
		{
			base.DataNavigator.FillChild(simpleContainer.Items);
			PrepareContainerForConverting(simpleContainer);
			base.FireSectionRendering(section);
			StandardPrinter.EvaluateRow(base.Evaluator,mylist);
			Point curPos  = BaseConverter.BaseConvert(mylist,simpleContainer,defaultLeftPos,currentPosition);
			AfterConverting (section);
			return curPos;
		}
		
		
		private  Point ConvertStandardRow(ExporterCollection mylist, BaseSection section, ISimpleContainer simpleContainer, int defaultLeftPos, Point currentPosition)
		{
			base.FillRow(simpleContainer);
			PrepareContainerForConverting(simpleContainer);
			base.FireSectionRendering(section);
			StandardPrinter.EvaluateRow(base.Evaluator,mylist);
			Point curPos = BaseConverter.BaseConvert(mylist,simpleContainer,defaultLeftPos,currentPosition);
			AfterConverting (section);
			return curPos;
		}
		
		
		
		private static void ShouldDrawBorder (BaseSection section,ExporterCollection list)
		{
			if (section.DrawBorder == true) {
				BaseRectangleItem br = BasePager.CreateDebugItem (section);
				BaseExportColumn bec = br.CreateExportColumn();
				bec.StyleDecorator.Location = section.Location;
				list.Insert(0,bec);
			}
		}
	}
}
