//-----------------------------------------------------------------------------------//
//           THIS FILE PROVIDES THE INTERACTION LOGIC FOR MainWindow.xaml            //
//-----------------------------------------------------------------------------------//

using System;
using Syncfusion.Pdf.Parsing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Syncfusion.Windows.PdfViewer;
using System.Collections.Generic;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Graphics;

namespace MyNotebook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private PdfLoadedDocument pdfloaded;
        private string NotebookName;
        private Stream docStream;
        public event PropertyChangedEventHandler PropertyChanged;
        private Dictionary<int, Dictionary<string, Rectangle>> selectedTextInformation;

        public MainWindow()
        {
            InitializeComponent();
            FontFamilyPicker.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontSizePicker.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        }

        public Stream DocumentStream
        {
            get
            {
                return docStream;
            }
            set
            {
                docStream = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DocumentStream"));
            }
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void FileButton_DropDownOpened(object sender, System.EventArgs e)
        {

        }
        //------------------------------------------------------------------------------------------------------------------------------//
        //                                                           File section (Save, SaveAs, Open) >> room for improvement          //
        //------------------------------------------------------------------------------------------------------------------------------//
        //---------------------------------------------------------------------------//
        //  Still working on implementation >> looking for a way to optimize process //
        //---------------------------------------------------------------------------//
        private void OpenPDFFile_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();

            // Get the selected file name and display in a TextBox. Load content of file in a TextBlock
            if (result == true)
            {
                var filepath = openFileDlg.FileName;
                pdfloaded = new PdfLoadedDocument(filepath);
                PdfViewer.Load(pdfloaded);
            }
        }  
        private void OpenNotesFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fddlg = new FolderBrowserDialog();
            DialogResult result = fddlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = fddlg.SelectedPath;
                string[] pdfFileName = Directory.GetFiles(folderName, "*.pdf");
                string[] notesFileName = Directory.GetFiles(folderName, "*.docx");
                foreach (string fileName in pdfFileName)
                {
                    PdfViewer.Load(fileName);
                    pdfloaded = new PdfLoadedDocument(fileName);
                }
                foreach (string fileName in notesFileName)
                {
                    Notebook.Load(fileName);
                }
            }

        }
        private void SavePDFFile_Click(object sender, RoutedEventArgs e)
        {
            string folderName = @"c:\\MyAppWPF_temp";
            string pathString = System.IO.Path.Combine(folderName, "MyAppWPF_ModifiedPDFs");
            string PDFfileName = PdfViewer.LoadedDocument.DocumentInformation.Title + "_Annotations.pdf";
            string PDFpathString = System.IO.Path.Combine(pathString, PDFfileName);
            PdfViewer.SaveDocumentCommand.Execute(PDFpathString);
        }
        private void SaveNotesFile_Click(object sender, RoutedEventArgs e)
        {
            // Creating directory
            string folderName = @"c:\\MyAppWPF_temp";
            NotebookName = PdfViewer.LoadedDocument.DocumentInformation.Title + "_Notebook";
            string pathString = System.IO.Path.Combine(folderName, NotebookName);

            if (!Directory.Exists(pathString))
            {
                System.IO.Directory.CreateDirectory(pathString);
            }
            string newPathString = pathString;
            // Create a file name for the file you want to create.
            string PDFfileName = PdfViewer.LoadedDocument.DocumentInformation.Title + "_Annotations.pdf";
            string noteFileName = PdfViewer.LoadedDocument.DocumentInformation.Title + "_Notes.docx";
            // Use Combine again to add the file name to the path.
            string PDFpathString = System.IO.Path.Combine(newPathString, PDFfileName);
            string notePathString = System.IO.Path.Combine(newPathString, noteFileName);
            PdfViewer.SaveDocumentCommand.Execute(PDFpathString);
            Notebook.Save(notePathString);
            
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            // Creating directory
            SaveFileDialog sfdlg = new SaveFileDialog();
            if (sfdlg.ShowDialog() == true)
            {
                string folderName = sfdlg.FileName;
                string NotesTitle = sfdlg.SafeFileName;
                NotebookName = folderName + "_Notebook";
                string pathString = System.IO.Path.Combine(folderName, NotebookName);
                if (!Directory.Exists(pathString))
                {
                    System.IO.Directory.CreateDirectory(pathString);
                }
                
                string newPathString = pathString;
                // Create a file name for the file you want to create.
                string PDFfileName = NotesTitle + "_Annotations.pdf";
                string noteFileName = NotesTitle + "_Notes.docx";
                // Use Combine again to add the file name to the path.
                string PDFpathString = System.IO.Path.Combine(pathString, PDFfileName);
                string notePathString = System.IO.Path.Combine(pathString, noteFileName);
                PdfViewer.SaveDocumentCommand.Execute(PDFpathString);
                Notebook.Save(notePathString);

            }
        }
        //------------------------------------------------------------------------------------------------------------------------------//
        //                                                                                                End of file section.          //
        //------------------------------------------------------------------------------------------------------------------------------//
        private void Notebook_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Notebook_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void FontFamilyPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        //---------------------------------------------------------------------------//
        //   Gets the whole selected text && its rectangular bounds for each page    //
        //           (separately if the selection is made on multiple pages)         //
        //---------------------------------------------------------------------------//
        private void PdfViewer_TextSelectionCompleted(object sender, TextSelectionCompletedEventArgs args)
        {
            string selectedText = args.SelectedText;
            selectedTextInformation = args.SelectedTextInformation;
        }

        //---------------------------------------------------------------------------//
        //  Still working on implementation >> the check is not in sync with viewer  //
        //      ** Button and/or button group is therefore curently disabled **      //
        //---------------------------------------------------------------------------//
        private void PopupComment_Click(object sender, RoutedEventArgs e)
        {
            int lastHighlightedPageIndex = 0;
            string selectedText = PdfViewer.SelectedText;
            //Get the whole selected text 

            if (PdfViewer.SelectedText != null)
            {
                foreach (var selectedTextInfo in selectedTextInformation)
                {
                    PdfLoadedPage page = PdfViewer.LoadedDocument.Pages[selectedTextInfo.Key - 1] as PdfLoadedPage;
                    lastHighlightedPageIndex = selectedTextInfo.Key;
                    Dictionary<string, Rectangle> SelectedTexts = selectedTextInfo.Value;
                   
                    foreach (var selectedtext in SelectedTexts)
                    {
                        //Add text markup annotation on the bounds of highlighting text
                        PdfTextMarkupAnnotation textmarkup = new PdfTextMarkupAnnotation(selectedtext.Value)
                        {
                            //Sets the markup annotation type as HighLight
                            TextMarkupAnnotationType = PdfTextMarkupAnnotationType.Highlight,
                            //Sets the content of the annotation
                            Text = selectedText,
                            //Sets the highlighting color
                            TextMarkupColor = new PdfColor(System.Drawing.Color.Yellow)
                        };
                        
                        //Add annotation into page
                        page.Annotations.Add(textmarkup);
                    }
                }

                //Save the document to disk. Save the LoadedDocument and reload into PdfViewerControl
                MemoryStream stream = new MemoryStream
                {
                    Position = 0
                };
                PdfViewer.LoadedDocument.Save(stream);
                PdfViewer.Load(stream);
                PdfViewer.GoToPageAtIndex(lastHighlightedPageIndex);
            }
        }

        //---------------------------------------------------------------------------//
        //          Used in Notebook for inserting shape style bullet points         //
        //---------------------------------------------------------------------------//
        private void ListBulletButton_Click(object sender, RoutedEventArgs e)
        {
            // Initializes a new abstract list instance.
            AbstractListAdv abstractListAdv = new AbstractListAdv(null)
            {
                AbstractListId = 1
            };

            // Defines new Bullet ListLevel instance.
            ListLevelAdv listLevel = new ListLevelAdv(abstractListAdv);
            listLevel.ParagraphFormat.LeftIndent = 6d;
            listLevel.ParagraphFormat.FirstLineIndent = 18d;
            listLevel.FollowCharacter = FollowCharacterType.Tab;
            listLevel.ListLevelPattern = ListLevelPattern.Bullet;
            listLevel.RestartLevel = 0;
            listLevel.StartAt = 0;

            // Defines Square bullet.
            listLevel.NumberFormat = "\uf0a7";
            listLevel.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("Wingdings");
            // Defines Arrow Bullet.
            listLevel.NumberFormat = "\u27a4";
            listLevel.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("Arial Unicode MS");
            // Defines Beginning of Bulleted List.
            listLevel.NumberFormat = "\uf0b7";
            listLevel.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("Symbol");
            // Adds list level to abstract list.
            abstractListAdv.Levels.Add(listLevel);

            // Adds abstract list to the document.
            Notebook.Document.AbstractLists.Add(abstractListAdv);

            // Creates a new list instance.
            ListAdv listAdv = new ListAdv(null)
            {
                ListId = 1,
                // Sets the abstract list Id for this list.
                AbstractListId = 1
            };

            if (Notebook.Selection != null)
            {
                LevelOverrideAdv levelOverride = new LevelOverrideAdv(listAdv)
                {
                    LevelNumber = 0,
                    StartAt = 0
                };
                listAdv.LevelOverrides.Add(levelOverride);
                Notebook.Document.Lists.Add(listAdv);
                Notebook.Selection.ParagraphFormat.SetList(listAdv);
                Notebook.Selection.ParagraphFormat.ListLevelNumber = 0;
            }
        }
        //---------------------------------------------------------------------------//
        //        Used in Notebook for inserting number style bullet points          //
        //---------------------------------------------------------------------------//
        private void ListNumberButton_Click(object sender, RoutedEventArgs e)
        {
            // Initializes a new abstract list instance.
            AbstractListAdv numListAdv = new AbstractListAdv(null)
            {
                AbstractListId = 2
            };

            // Defines new Bullet ListLevel instance.
            ListLevelAdv listLevel = new ListLevelAdv(numListAdv);
            listLevel.ParagraphFormat.LeftIndent = 6d;
            listLevel.ParagraphFormat.FirstLineIndent = 18d;
            listLevel.FollowCharacter = FollowCharacterType.Tab;
            listLevel.ListLevelPattern = ListLevelPattern.Number;
            listLevel.NumberFormat = "%1.";

            listLevel.RestartLevel = 1;
            listLevel.StartAt = 1;

            // Adds list level to abstract list.
            numListAdv.Levels.Add(listLevel);

            // Adds abstract list to the document.
            Notebook.Document.AbstractLists.Add(numListAdv);

            // Creates a new list instance.
            ListAdv numlistAdv = new ListAdv(null)
            {
                ListId = 2,
                // Sets the abstract list Id for this list.
                AbstractListId = 2
            };

            if (Notebook.Selection != null)
            {
                Notebook.Document.Lists.Add(numlistAdv);
                Notebook.Selection.ParagraphFormat.SetList(numlistAdv);
                Notebook.Selection.ParagraphFormat.ListLevelNumber = 1;
            }
        }
        //---------------------------------------------------------------------------//
        //  Still working on implementation >> the check is not in sync with viewer  //
        //      ** Button and/or button group is therefore curently disabled **      //
        //---------------------------------------------------------------------------//
        private void NotebookOtherViewCont_Checked(object sender, RoutedEventArgs e)
        {
            Notebook.LayoutType = LayoutType.Continuous;
        }
        //---------------------------------------------------------------------------//
        //  Still working on implementation >> the check is not in sync with viewer  //
        //      ** Button and/or button group is therefore curently disabled **      //
        //---------------------------------------------------------------------------//
        private void NotebookOtherViewCPages_Checked(object sender, RoutedEventArgs e)
        {
            Notebook.LayoutType = LayoutType.Pages;
        }
        //---------------------------------------------------------------------------//
        //  Currently Syncfusion does not have an accessible command for superscript //
        //      ** Button and/or button group is therefore curently disabled **      //
        //---------------------------------------------------------------------------//
        private void NoteSuperScriptButton_Click(object sender, RoutedEventArgs e)
        {
            // ***   ***   ***   ***   ***   ***   ***   ***   ***   ***   *** //
        }
        //---------------------------------------------------------------------------//
        //  Currently Syncfusion does not have an accessible command for superscript //
        //      ** Button and/or button group is therefore curently disabled **      //
        //---------------------------------------------------------------------------//
        private void NoteSubScriptButton_Click(object sender, RoutedEventArgs e)
        {
            // ***   ***   ***   ***   ***   ***   ***   ***   ***   ***   *** //
        }
    }
}
//-----------------------------------------------------------------------------------//
