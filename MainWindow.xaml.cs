using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace CSV_Converter
{
    /**
     * class that defines the interaction logic for the wpf defined by MainWindow.xaml
     */
    public partial class MainWindow : Window
    {
        /**
         * Constructor
         */
        public MainWindow()
        {
            m_Converter = new CSVConverter();
            m_name = "";
            m_user = "";
            m_map = "";
            m_model = "";
            m_prevOpenDir = AppDomain.CurrentDomain.BaseDirectory;
            m_prevConvertDir = AppDomain.CurrentDomain.BaseDirectory;

            string errorMsg;
            if (!m_Converter.Init(out errorMsg))
            {
                MessageBox.Show(errorMsg);
                /* do not show the UI */
                Close();
            }
            else
            {
                InitializeComponent();
                m_FileOpened = false;
            }
        }

        /**
         * Click-Event of the "Open File" button.
         * Opens an Explorer window such that the user can choose the CSV file that should be opened and parsed. 
         */
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.InitialDirectory = m_prevOpenDir;

            if (openFileDialog.ShowDialog() == true)
            {
                /* use the directory as the starting point for the next opening of a file */
                m_prevOpenDir = System.IO.Path.GetDirectoryName(openFileDialog.FileName);

                if (m_Converter.OpenAndParse(openFileDialog.FileName, out errorMsg))
                {
                    m_FileOpened = true;
                    txtStatus.Text = "File opened.";
                    txtStatus.Background = Brushes.LightGreen;
                }
                else
                {
                    txtStatus.Text = errorMsg;
                    txtStatus.Background = Brushes.Red;
                }
            }
            else
            {
                txtStatus.Text = "Error: No File chosen.";
                txtStatus.Background = Brushes.Red;
            }
        }

        /**
         * Click-Event of the "Convert" button.
         * Opens an SaveAs
         * Converts the existing internal geometries into the CALIGO conform format.
         */ 
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            /* check if a file was previously opened */
            if (!m_FileOpened)
            {
                txtStatus.Text = "Error: It is necessary to open a file prior to the conversion.";
                txtStatus.Background = Brushes.Red;
                return;
            }

            string errorMsg;
            /* create save as window */
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Document";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.Filter = "Text documents (.txt)|*.txt";
            saveFileDialog.InitialDirectory = m_prevConvertDir;

            if (saveFileDialog.ShowDialog() == true)
            {
                /* use the directory as the starting point for the next save file */
                m_prevConvertDir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);

                if (m_Converter.Convert(saveFileDialog.FileName, m_map, m_model, m_user, m_name, out errorMsg))
                {
                    txtStatus.Text = "File with converted content created.";
                    txtStatus.Background = Brushes.LightGreen;
                }
                else
                {
                    txtStatus.Text = errorMsg;
                    txtStatus.Background = Brushes.Red;
                }
            }
            else
            {
                txtStatus.Text = "Error: No conversion filename chosen.";
                txtStatus.Background = Brushes.Red;
            }
        }

        private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_user = userInput.Text;
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_name = nameInput.Text;
        }

        private void MapInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_map = mapInput.Text;
        }

        private void ModelInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_model = modelInput.Text;
        }

        /* members */
        private CSVConverter m_Converter;       //!< class handling the conversion of the data of csv files into a format that can be interpreted by CALIGO
        private bool m_FileOpened;              //!< whether a file has been opened
        private string m_user;                  //!< header info: user id
        private string m_name;                  //!< header info: user name
        private string m_model;                 //!< header info: CATIA model name
        private string m_map;                   //!< header info: CATIA map name
        private string m_prevOpenDir;
        private string m_prevConvertDir;

    }
}
