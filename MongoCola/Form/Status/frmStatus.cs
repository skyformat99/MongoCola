﻿using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MongoCola.Module;
using MongoDB.Bson;
using MongoUtility.Basic;

namespace MongoCola
{
    public partial class frmStatus : Form
    {
        public frmStatus()
        {
            InitializeComponent();
        }

        private void frmStatus_Load(object sender, EventArgs e)
        {
            Icon = GetSystemIcon.ConvertImgToIcon(ResourceLib.Properties.Resources.KeyInfo);
            String strType = MongoUtility.Core.RuntimeMongoDBContext.SelectTagType;
            var DocStatus = new BsonDocument();
            cmbChartField.Visible = false;
            chartResult.Visible = false;
            btnOpCnt.Visible = false;
            switch (strType)
            {
                case ConstMgr.SERVER_TAG:
                case ConstMgr.SINGLE_DB_SERVER_TAG:
            		if (MongoUtility.Core.RuntimeMongoDBContext.GetCurrentServerConfig().LoginAsAdmin)
                    {
                        DocStatus =
                            CommandHelper.ExecuteMongoSvrCommand(CommandHelper.serverStatus_Command,
                                MongoUtility.Core.RuntimeMongoDBContext.GetCurrentServer()).Response;
                        trvStatus.Height = trvStatus.Height*2;
                    }
                    if (strType == ConstMgr.SERVER_TAG)
                    {
                        btnOpCnt.Visible = true;
                    }
                    break;
                case ConstMgr.DATABASE_TAG:
                case ConstMgr.SINGLE_DATABASE_TAG:
                    DocStatus = MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetStats().Response.ToBsonDocument();
                    cmbChartField.Visible = true;
                    chartResult.Visible = true;

                    cmbChartField.Items.Add("AverageObjectSize");
                    cmbChartField.Items.Add("DataSize");
                    cmbChartField.Items.Add("ExtentCount");
                    cmbChartField.Items.Add("IndexCount");
                    cmbChartField.Items.Add("LastExtentSize");
                    //MaxDocuments仅在CapedCollection时候有效
                    //cmbChartField.Items.Add("MaxDocuments");
                    cmbChartField.Items.Add("ObjectCount");
                    cmbChartField.Items.Add("PaddingFactor");
                    cmbChartField.Items.Add("StorageSize");
                    cmbChartField.SelectedIndex = 0;
                    try
                    {
                        RefreshDBStatusChart("StorageSize");
                    }
                    catch (Exception ex)
                    {
                        Common.Utility.ExceptionDeal(ex);
                    }
                    break;
                case ConstMgr.COLLECTION_TAG:
                    DocStatus = MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().Response.ToBsonDocument();
                    //图形化初始化
                    chartResult.Visible = true;

                    chartResult.Series.Clear();
                    chartResult.Titles.Clear();
                    var SeriesResult = new Series("Usage");
                    var dpDataSize = new DataPoint(0, MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().DataSize)
                    {
                        LegendText = "DataSize",
                        LegendToolTip = "DataSize",
                        ToolTip = "DataSize"
                    };
                    SeriesResult.Points.Add(dpDataSize);

                    var dpTotalIndexSize = new DataPoint(0,
                        MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().TotalIndexSize)
                    {
                        LegendText = "TotalIndexSize",
                        LegendToolTip = "TotalIndexSize",
                        ToolTip = "TotalIndexSize"
                    };
                    SeriesResult.Points.Add(dpTotalIndexSize);

                    var dpFreeSize = new DataPoint(0, MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().StorageSize -
                                                      MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().TotalIndexSize -
                                                      MongoUtility.Core.RuntimeMongoDBContext.GetCurrentCollection().GetStats().DataSize)
                    {
                        LegendText = "FreeSize",
                        LegendToolTip = "FreeSize",
                        ToolTip = "FreeSize"
                    };
                    SeriesResult.Points.Add(dpFreeSize);

                    SeriesResult.ChartType = SeriesChartType.Pie;
                    chartResult.Series.Add(SeriesResult);
                    chartResult.Titles.Add("Usage");

                    break;
                default:
                    if (MongoUtility.Core.RuntimeMongoDBContext.GetCurrentServerConfig().LoginAsAdmin)
                    {
                        DocStatus =
                            CommandHelper.ExecuteMongoSvrCommand(CommandHelper.serverStatus_Command,
                                MongoUtility.Core.RuntimeMongoDBContext.GetCurrentServer()).Response;
                        trvStatus.Height = trvStatus.Height*2;
                    }
                    break;
            }
            if (!SystemManager.IsUseDefaultLanguage)
            {
                Text = SystemManager.guiConfig.MStringResource.GetText(StringResource.TextType.Main_Menu_Mangt_Status);
                cmdClose.Text = SystemManager.guiConfig.MStringResource.GetText(StringResource.TextType.Common_Close);
            }
            MongoGUICtl.UIHelper.FillDataToTreeView(strType, trvStatus, DocStatus);
            trvStatus.DatatreeView.Nodes[0].Expand();
        }

        private void btnOpCnt_Click(object sender, EventArgs e)
        {
            Common.Utility.OpenForm(new frmServerMonitor(), true, true);
        }

        private void RefreshDBStatusChart(String strField)
        {
            //图形化初始化
            chartResult.Series.Clear();
            chartResult.Titles.Clear();
            var SeriesResult = new Series(strField);
            foreach (String colName in MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollectionNames())
            {
                DataPoint ColPoint;
                switch (strField)
                {
                    case "AverageObjectSize":
                        try
                        {
                            //如果没有任何对象的时候，平均值无法取得
                            ColPoint = new DataPoint(0,
                                MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().AverageObjectSize);
                        }
                        catch (Exception ex)
                        {
                            ColPoint = new DataPoint(0, 0);
                            Common.Utility.ExceptionDeal(ex);
                        }
                        break;
                    case "DataSize":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().DataSize);
                        break;
                    case "ExtentCount":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().ExtentCount);
                        break;
                        //case "Flags":
                        //    ColPoint = new DataPoint(0, MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().Flags);
                        //    break;
                    case "IndexCount":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().IndexCount);
                        break;
                    case "LastExtentSize":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().LastExtentSize);
                        break;
                        //case "MaxDocuments":
                        //    仅在CappedCollection时候有效 
                        //    ColPoint = new DataPoint(0, MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().MaxDocuments);
                        //    break;
                    case "ObjectCount":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().ObjectCount);
                        break;
                    case "PaddingFactor":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().PaddingFactor);
                        break;
                    case "StorageSize":
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().StorageSize);
                        break;
                    default:
                        ColPoint = new DataPoint(0,
                            MongoUtility.Core.RuntimeMongoDBContext.GetCurrentDataBase().GetCollection(colName).GetStats().StorageSize);
                        break;
                }

                ColPoint.LegendText = colName;
                ColPoint.LabelToolTip = colName;
                ColPoint.ToolTip = colName;
                SeriesResult.Points.Add(ColPoint);
            }
            //图形化加载

            SeriesResult.ChartType = SeriesChartType.Pie;
            chartResult.Series.Add(SeriesResult);
            chartResult.Titles.Add(strField);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmbChartField_SelectedIndexChanged(object sender, EventArgs e)
        {
            String strType = MongoUtility.Core.RuntimeMongoDBContext.SelectTagType;
            switch (strType)
            {
                case ConstMgr.DATABASE_TAG:
                case ConstMgr.SINGLE_DATABASE_TAG:
                    try
                    {
                        RefreshDBStatusChart(cmbChartField.Text);
                    }
                    catch (Exception ex)
                    {
                        Common.Utility.ExceptionDeal(ex);
                    }
                    break;
            }
        }
    }
}