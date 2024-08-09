
' Define models to match the JSON structure
Public Class ReportRequest
    Public Property report As ReportInfo
    Public Property ExportFile As ExportFileInfo
    Public Property reportData As ReportData
End Class

Public Class ReportInfo
    Public Property reportformat As String
    Public Property reportFile As String
End Class

Public Class ExportFileInfo
    Public Property format As String
    Public Property ExportFileName As String
    Public Property ExportFilePath As String
    Public Property SendAllstoragePath As Boolean
    Public Property DeleteAfterDate As Integer
End Class

Public Class ReportData
    Public Property reportSqlType As Boolean
    Public Property reportTableName As String
    Public Property sqlQuery As String
    Public Property parameters As List(Of ReportParameter)
End Class

Public Class ReportParameter
    Public Property name As String
    Public Property value As String
End Class
Public Class DatabaseConfig
    Public Property database As DatabaseDetails
End Class

Public Class DatabaseDetails
    Public Property server As String
    Public Property databaseName As String
    Public Property authentication As AuthenticationDetails
End Class

Public Class AuthenticationDetails
    Public Property method As Boolean
    Public Property username As String
    Public Property password As String
End Class

