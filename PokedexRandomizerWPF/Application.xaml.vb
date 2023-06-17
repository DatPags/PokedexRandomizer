Class Application
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        Dim logDir As String = IO.Path.Combine(PokedexRandomizerLib.Util.DIRECTORY_BASE, "logs")
        PokedexRandomizerLib.Util.CreateDirectory(logDir)
        Dim filename As String = "error" + Date.UtcNow.ToString("MM dd yyyy HH mm ss").Replace(" ", "") + ".log"
        Dim path As String = IO.Path.Combine(logDir, filename)
        Dim errorText As String = e.Exception.GetType().ToString() + ": " + e.Exception.Message + vbCrLf + e.Exception.StackTrace
        IO.File.WriteAllText(path, errorText)
        Dim result = MessageBox.Show("An error has occurred causing the program to stop. Details of the error were written to the following file: " + vbCrLf + path + vbCrLf + vbCrLf + "Contact the developer with these details if this is a recurring issue.", "Error", MessageBoxButton.OK)
    End Sub
End Class
