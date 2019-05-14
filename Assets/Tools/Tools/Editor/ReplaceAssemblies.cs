using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class ReplaceAssemblies : ScriptableSingleton<ReplaceAssemblies>
{

    public static string ASSEMBLY_EXTENSION = ".dll";
    public static string ASSEMBLY_DEFINITION_EXTENSION = ".asmdef";

    [SerializeField]
    private List<String> assembliesFilesToReplace = new List<string>();

    [SerializeField]
    private List<string> pathsOfAssemblyFilesInAssetFolder = new List<string>();
    [SerializeField]
    private List<string> pathsOfAssemblyFilesCreatedByUnity = new List<string>();

    [SerializeField]
    private string tempSourceFilePath;

    public string TempSourceFilePath
    {
        get
        {
            if (String.IsNullOrEmpty(tempSourceFilePath))
            {
                tempSourceFilePath = FileUtil.GetUniqueTempPathInProject();
            }

            return tempSourceFilePath;
        }
    }

    void OnEnable()
    {
        Debug.Log("temp dir : " + TempSourceFilePath);
    }

    public void ReplaceAssembly(string assemblyPath, CompilerMessage[] messages)
    {
        string assemblyFileName = assembliesFilesToReplace.Find(assembly => assemblyPath.EndsWith(assembly));
        // is this one of the assemblies we want to replace ?
        if (!String.IsNullOrEmpty(assemblyFileName))
        {
            string[] assemblyDefinitionFilePaths = Directory.GetFiles(".", Path.GetFileNameWithoutExtension(assemblyFileName) + ASSEMBLY_DEFINITION_EXTENSION, SearchOption.AllDirectories);
            if (assemblyDefinitionFilePaths.Length > 0)
            {
                string assemblyDefinitionFilePath = assemblyDefinitionFilePaths[0];
                ReplaceAssembly(assemblyDefinitionFilePath);
            }
        }
    }

    public void AddAssemblyFileToReplace(string assemblyFile)
    {
        assembliesFilesToReplace.Add(assemblyFile);
    }

    private void ReplaceAssembly(string assemblyDefinitionFilePath)
    {
        Debug.LogFormat("Replacing scripts for assembly definition file {0}", assemblyDefinitionFilePath);
        string asmdefDirectory = Path.GetDirectoryName(assemblyDefinitionFilePath);
        string assemblyName = Path.GetFileNameWithoutExtension(assemblyDefinitionFilePath);
        Assembly assemblyToReplace = CompilationPipeline.GetAssemblies().ToList().Find(assembly => assembly.name.ToLower().Equals(assemblyName.ToLower()));
        string assemblyPath = assemblyToReplace.outputPath;
        string assemblyFileName = Path.GetFileName(assemblyPath);
        string[] assemblyFilePathInAssets = Directory.GetFiles("./Assets", assemblyFileName, SearchOption.AllDirectories);
        if (assemblyFilePathInAssets.Length <= 0)
        {
            foreach (string sourceFile in assemblyToReplace.sourceFiles)
            {
                string tempScriptPath = Path.Combine(TempSourceFilePath, sourceFile);
                Directory.CreateDirectory(Path.GetDirectoryName(tempScriptPath));
                if (!File.Exists(sourceFile))
                    Debug.LogErrorFormat("File {0} does not exist while the assembly {1} references it.", sourceFile, assemblyToReplace.name);
                Debug.Log("will move " + sourceFile + " to " + tempScriptPath);

                FileUtil.MoveFileOrDirectory(sourceFile, tempScriptPath);
            }
            string finalAssemblyPath = Path.Combine(asmdefDirectory, assemblyFileName);
            Debug.Log("will move " + assemblyPath + " to " + finalAssemblyPath);
            FileUtil.MoveFileOrDirectory(assemblyPath, finalAssemblyPath);
            pathsOfAssemblyFilesInAssetFolder.Add(finalAssemblyPath);
            pathsOfAssemblyFilesCreatedByUnity.Add(assemblyPath);
        }
        else
        {
            Debug.Log("Already found an assembly file named " + assemblyFileName + " in asset folder");
        }
    }

    [MenuItem("Tools/Replace Assembly")]
    public static void ReplaceAssemblyMenu()
    {
        string assemblyDefinitionFilePath = EditorUtility.OpenFilePanel(
            title: "Select Assembly Definition File",
            directory: Application.dataPath,
            extension: ASSEMBLY_DEFINITION_EXTENSION.Substring(1));
        if (assemblyDefinitionFilePath.Length == 0)
            return;

        instance.ReplaceAssembly(assemblyDefinitionFilePath);

    }

    [MenuItem("Tools/Revert Replace all Assemblies")]
    public static void RevertReplaceAssembliesMenu()
    {
        instance.RevertReplaceAssemblies();
    }

    private void RevertReplaceAssemblies()
    {
        Debug.Log(pathsOfAssemblyFilesInAssetFolder.Count);
        for (int i = 0; i < pathsOfAssemblyFilesInAssetFolder.Count; ++i)
        {
            Debug.Log("will move " + pathsOfAssemblyFilesInAssetFolder[i] + " back to " + pathsOfAssemblyFilesCreatedByUnity[i]);
            FileUtil.MoveFileOrDirectory(pathsOfAssemblyFilesInAssetFolder[i], pathsOfAssemblyFilesCreatedByUnity[i]);
        }
        if (Directory.Exists(TempSourceFilePath))
        {
            string[] scriptFilesInTempDir = Directory.GetFiles(TempSourceFilePath, "*", SearchOption.AllDirectories);
            foreach (String scriptFileInTempDir in scriptFilesInTempDir)
            {
                // remove the temp directories prefix and the directory separator character
                string originalScriptFilePath = scriptFileInTempDir.Substring(TempSourceFilePath.Length + 1);
                Debug.Log("will move " + scriptFileInTempDir + " back to " + originalScriptFilePath);
                FileUtil.MoveFileOrDirectory(scriptFileInTempDir, originalScriptFilePath);
            }

        }
        pathsOfAssemblyFilesInAssetFolder = new List<string>();
        pathsOfAssemblyFilesCreatedByUnity = new List<string>();
    }
}
