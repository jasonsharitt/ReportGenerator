Imports System.Data.SqlClient

Public Class TableKreator
    Public Function CreateTables(ByVal cmdtxt As String, ByVal psnfile As String, ByVal firrow As String, ByVal ffile As String)
        'This function executes the SQL stored procedures that create the nessecary tables required by the reports selected.

        'Reminder: Position-based reports need to have the double quotes taken out from the position files before 
        '          tables can be created.

        Dim sql2 As New SqlCommand()
        Dim connectionstring As String = "Data Source=(local)\SQLExpress,34524;Initial Catalog=Analysis;Integrated Security=True;Connect Timeout=60"
        Dim connection As New SqlConnection(connectionstring)
        Try
            With sql2
                .CommandTimeout = 0
                .CommandText = cmdtxt
                .CommandType = CommandType.StoredProcedure
                If cmdtxt.Contains("Position") Then
                    .Parameters.Add("@positionfilepath", SqlDbType.NVarChar, 4000).Value = psnfile
                ElseIf cmdtxt.Contains("RAW") Then
                    .Parameters.Add("@rawfilepath", SqlDbType.NVarChar, 4000).Value = psnfile
                    .Parameters.Add("@firstrow", SqlDbType.NVarChar, 5).Value = firrow
                    .Parameters.Add("@formatfile", SqlDbType.NVarChar, 4000).Value = ffile
                End If
                .Connection = connection
                .Connection.Open()
                .ExecuteNonQuery()
                .Connection.Close()
                .Parameters.Clear()
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
        Return True
    End Function
End Class
