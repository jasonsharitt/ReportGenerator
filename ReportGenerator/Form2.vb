Public Class Form2

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Form1.BackgroundWorker1.WorkerSupportsCancellation = True Then
            BackgroundWorker1.CancelAsync()
        End If
    End Sub
End Class