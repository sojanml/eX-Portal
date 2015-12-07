using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text;
using System.IO;

namespace eX_Portal.exLogic {
  public class SimpleParser {
    public ColumnInfoList aColumnInfoList = new ColumnInfoList();
    public StringBuilder TheData = new StringBuilder();

    public void Parse(String SQL) {
      TSql120Parser SqlParser = new TSql120Parser(false);

      IList<ParseError> parseErrors;
      TSqlFragment result = SqlParser.Parse(new StringReader(SQL),
                                            out parseErrors);

      TSqlScript SqlScript = result as TSqlScript;

      foreach (TSqlBatch sqlBatch in SqlScript.Batches) {
        foreach (TSqlStatement sqlStatement in sqlBatch.Statements) {
          ProcessViewStatementBody(sqlStatement);
        }
      }
    }


    private void ProcessViewStatementBody(TSqlStatement sqlStatement) {

      ViewStatementBody aViewStatementBody = (ViewStatementBody)sqlStatement;

        AddLogText("Columns:");
        foreach (Identifier aColumnIdentifier in aViewStatementBody.Columns) {
          AddLogText(string.Format("Column:{0}", aColumnIdentifier.Value));
          aColumnInfoList.ColumnList.Add(new ColumnInfo { Alias = aColumnIdentifier.Value });
        }

        AddLogText("");
        AddLogText("QueryExpression SelectElements:");
        SelectStatement aSelectStatement = aViewStatementBody.SelectStatement;
        QueryExpression aQueryExpression = aSelectStatement.QueryExpression;
        if (aQueryExpression.GetType() == typeof(QuerySpecification)) {
          QuerySpecification aQuerySpecification = (QuerySpecification)aQueryExpression;
          int aSelectElementID = 0;
          foreach (SelectElement aSelectElement in aQuerySpecification.SelectElements) {
            if (aSelectElement.GetType() == typeof(SelectScalarExpression)) {
              SelectScalarExpression aSelectScalarExpression = (SelectScalarExpression)aSelectElement;

              string identStr = string.Empty;
              IdentifierOrValueExpression aIdentifierOrValueExpression =
                  aSelectScalarExpression.ColumnName;
              if (aIdentifierOrValueExpression != null) {
                if (aIdentifierOrValueExpression.ValueExpression == null) {
                  AddLogText(string.Format("Identifier={0}",
                      aIdentifierOrValueExpression.Identifier.Value));
                  identStr = aIdentifierOrValueExpression.Identifier.Value;
                } else {
                  AddLogText("Expression");
                }
              }
              aColumnInfoList.AddIfNeeded(aSelectElementID, identStr);

              ScalarExpression aScalarExpression = aSelectScalarExpression.Expression;
              PrintSelectScalarExperssionRecurse(aSelectElementID, aScalarExpression);
            } else {
              aColumnInfoList.AddIfNeeded(aSelectElementID,
                  "Error, something else than SelectScalarExpression found");
              AddLogText("We only support SelectScalarExpression.");
            }
            aSelectElementID = aSelectElementID + 1;
            AddLogText("");
          }
          AddLogText("");
          AddLogText("Table References:");
          FromClause aFromClause = aQuerySpecification.FromClause;
          foreach (TableReference aTableReference in aFromClause.TableReferences) {
            PrintTableReferenceRecurse(aTableReference);
          }
        }
        aColumnInfoList.FillEmptyAlias();

    }//function

    private string MultiPartIdentifierToString(int aSelectElementID, MultiPartIdentifier aMultiPartIdentifier) {
      String res = String.Empty;
      foreach (Identifier aIdentifier in aMultiPartIdentifier.Identifiers) {
        if (String.IsNullOrEmpty(res)) {
          res = aIdentifier.Value;
        } else {
          res = res + "." + aIdentifier.Value;
        }
      }
      aColumnInfoList.AddRefereceIdentifier(aSelectElementID, aMultiPartIdentifier);
      return res;
    }

    private void PrintSelectScalarExperssionRecurse(int aSelectElementID, ScalarExpression aScalarExpression) {
      if (aScalarExpression.GetType() == typeof(ColumnReferenceExpression)) {
        ColumnReferenceExpression aColumnReferenceExpression = (ColumnReferenceExpression)aScalarExpression;
        AddLogText(string.Format("ColumnType={0}", aColumnReferenceExpression.ColumnType));
        MultiPartIdentifier aMultiPartIdentifier = aColumnReferenceExpression.MultiPartIdentifier;
        AddLogText(string.Format("Reference Identifier={0}",
            MultiPartIdentifierToString(aSelectElementID, aMultiPartIdentifier)));
      } else if (aScalarExpression.GetType() == typeof(ConvertCall)) {
        ConvertCall aConvertCall = (ConvertCall)aScalarExpression;
        ScalarExpression aScalarExpressionParameter = aConvertCall.Parameter;
        PrintSelectScalarExperssionRecurse(aSelectElementID, aScalarExpressionParameter);
      } else {
        AddLogText(String.Format("Not supported Expression:{0}", aScalarExpression.GetType()));
      }
    }

    private void PrintTableReferenceRecurse(TableReference aTableReference) {
      if (aTableReference.GetType() == typeof(NamedTableReference)) {
        NamedTableReference aNamedTableReference = (NamedTableReference)aTableReference;
        Identifier aAliasIdentifier = aNamedTableReference.Alias;
        SchemaObjectName aSchemaObjectName = aNamedTableReference.SchemaObject;
        AddLogText(string.Format("Table Reference Schema.Base={0}.{1}",
            (aSchemaObjectName.SchemaIdentifier != null) ? aSchemaObjectName.SchemaIdentifier.Value : "",
            (aSchemaObjectName.BaseIdentifier != null) ? aSchemaObjectName.BaseIdentifier.Value : "")
            );
        //foreach (Identifier aSchemaObjectNameIdentifier in aSchemaObjectName.Identifiers)
        //{
        //    AddText(string.Format("Table Reference Identifier={0}", aSchemaObjectNameIdentifier.Value));
        //}
        if (aAliasIdentifier != null) {
          AddLogText(string.Format("Table Reference Alias:{0}", aAliasIdentifier.Value));
        }
        aColumnInfoList.AddTableReference(aSchemaObjectName, aAliasIdentifier);
      }
      if (aTableReference.GetType() == typeof(QualifiedJoin)) {
        QualifiedJoin aQualifiedJoin = (QualifiedJoin)aTableReference;
        AddLogText(string.Format("Table Reference QualifiedJoinType ={0}", aQualifiedJoin.QualifiedJoinType));
        PrintTableReferenceRecurse(aQualifiedJoin.FirstTableReference);
        PrintTableReferenceRecurse(aQualifiedJoin.SecondTableReference);
      }
      if (aTableReference.GetType() == typeof(JoinTableReference)) {
        JoinTableReference aJoinTableReference = (JoinTableReference)aTableReference;
        PrintTableReferenceRecurse(aJoinTableReference.FirstTableReference);
        PrintTableReferenceRecurse(aJoinTableReference.SecondTableReference);
      }
    }

    private void AddLogText(string aLine) {
      TheData.AppendLine(aLine);
    }
    
    public class ColumnInfo {
      private string _Alias = String.Empty;
      private string _ReferencedTableDatabase = String.Empty;
      private string _ReferencedTableName = String.Empty;
      private string _ReferencedTableSchema = String.Empty;
      private string _ReferencedTableServer = String.Empty;
      private string _TableAlias = String.Empty;
      private string _TableColumnName = String.Empty;

      public string Alias {
        get { return _Alias; }
        set { _Alias = value; }
      }

      public string TableAlias {
        get { return _TableAlias; }
        set { _TableAlias = value; }
      }

      public string TableColumnName {
        get { return _TableColumnName; }
        set { _TableColumnName = value; }
      }

      public string ReferencedTableServer {
        get { return _ReferencedTableServer; }
        set { _ReferencedTableServer = value; }
      }

      public string ReferencedTableDatabase {
        get { return _ReferencedTableDatabase; }
        set { _ReferencedTableDatabase = value; }
      }

      public string ReferencedTableSchema {
        get { return _ReferencedTableSchema; }
        set { _ReferencedTableSchema = value; }
      }

      public string ReferencedTableName {
        get { return _ReferencedTableName; }
        set { _ReferencedTableName = value; }
      }
    }

    public class ColumnInfoList {
      private List<ColumnInfo> _ColumnList = new List<ColumnInfo>();

      public List<ColumnInfo> ColumnList {
        get { return _ColumnList; }
        set { _ColumnList = value; }
      }

      public void FillEmptyAlias() {
        foreach (ColumnInfo aColumnInfo in ColumnList) {
          if (string.IsNullOrWhiteSpace(aColumnInfo.Alias)) {
            aColumnInfo.Alias = aColumnInfo.TableColumnName;
          }
        }
      }

      public void AddIfNeeded(int aSelectElementID, string aIdentifier) {
        if (ColumnList.Count > aSelectElementID) {
        } else {
          ColumnList.Add(new ColumnInfo { Alias = aIdentifier });
        }
      }

      public void AddRefereceIdentifier(int aSelectElementID, MultiPartIdentifier aMultiPartIdentifier) {
        if (ColumnList.Count > aSelectElementID) {
          ColumnInfo aColumnInfo = ColumnList[aSelectElementID];
          int aIdentIdx = 0;
          foreach (Identifier aIdentifier in aMultiPartIdentifier.Identifiers) {
            if (aMultiPartIdentifier.Identifiers.Count == 2) {
              if (aIdentIdx == 0)
                aColumnInfo.TableAlias = aIdentifier.Value;
              if (aIdentIdx == 1)
                aColumnInfo.TableColumnName = aIdentifier.Value;
            }
            if (aMultiPartIdentifier.Identifiers.Count == 3) {
              if (aIdentIdx == 0)
                aColumnInfo.ReferencedTableSchema = aIdentifier.Value;
              if (aIdentIdx == 1)
                aColumnInfo.ReferencedTableName = aIdentifier.Value;
              if (aIdentIdx == 2)
                aColumnInfo.TableColumnName = aIdentifier.Value;
            }

            aIdentIdx = aIdentIdx + 1;
          }
        }
      }

      public void AddTableReference(SchemaObjectName aSchemaObjectName, Identifier aAliasIdentifier) {
        if (ColumnList.Count > 0) {
          foreach (ColumnInfo aColumnInfo in ColumnList) {
            if (aAliasIdentifier != null &&
                aColumnInfo.TableAlias.ToLower() == aAliasIdentifier.Value.ToLower()) {
              if (aSchemaObjectName.ServerIdentifier != null)
                aColumnInfo.ReferencedTableServer = aSchemaObjectName.ServerIdentifier.Value;
              if (aSchemaObjectName.DatabaseIdentifier != null)
                aColumnInfo.ReferencedTableDatabase = aSchemaObjectName.DatabaseIdentifier.Value;
              if (aSchemaObjectName.SchemaIdentifier != null)
                aColumnInfo.ReferencedTableSchema = aSchemaObjectName.SchemaIdentifier.Value;
              if (aSchemaObjectName.BaseIdentifier != null)
                aColumnInfo.ReferencedTableName = aSchemaObjectName.BaseIdentifier.Value;
            } else if ((aAliasIdentifier == null) &&
                       (aSchemaObjectName.BaseIdentifier != null) &&
                       (aSchemaObjectName.BaseIdentifier.Value.ToLower() == aColumnInfo.TableAlias.ToLower())) {
              if (aSchemaObjectName.ServerIdentifier != null)
                aColumnInfo.ReferencedTableServer = aSchemaObjectName.ServerIdentifier.Value;
              if (aSchemaObjectName.DatabaseIdentifier != null)
                aColumnInfo.ReferencedTableDatabase = aSchemaObjectName.DatabaseIdentifier.Value;
              if (aSchemaObjectName.SchemaIdentifier != null)
                aColumnInfo.ReferencedTableSchema = aSchemaObjectName.SchemaIdentifier.Value;
              if (aSchemaObjectName.BaseIdentifier != null)
                aColumnInfo.ReferencedTableName = aSchemaObjectName.BaseIdentifier.Value;
            }
          }
        }
      }
    }
  }
}