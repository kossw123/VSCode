using System.Text;
using static System.Console;

//MainClass<int> main = new MainClass<int>(123123);
//Logger logger = new Logger();

public class MainClass<T>
{
    List<Compound<T>> compoundList = new List<Compound<T>>();
}
public class State<T> : EventArgs
{
    public enum FlagState
    {
        None = 0,
        Validate = 1,
        InValidate = 2
    }

    T data;
    FlagState flag = FlagState.Validate;
    int validateCount = 0;

    T Data
    {
        get 
        { 
            if(data is not null)
                return data; 
            else
                throw new Exception(nameof(data));
        }
        set
        {
            data = value switch
            {
                FlagState.None              => default(T)!,
                FlagState.Validate          => value,
                FlagState.InValidate        => throw new Exception(nameof(value)),
                _                           => throw new Exception(nameof(value))
            };
        }
    }

    public State(T input) => this.data = input;

    /// 이런식으로 할거면 생성자에서 형변환을 미리 알려줘야 한다.
    /// 근데 generic도 해당이 되나?
    /// 이 코드는 opeator overloading이 먼저 실행되고 그 다음 실행된다.
    /// public static implicit operator T(State<T> value) => value.Data is not null ? (T)Convert.ChangeType(value.Data, typeof(T)) : throw new Exception(nameof(value));
    /// 역참조를 막아야 한다.
    
    /// 아래 코드는 사용자 정의 암시적 변환 연산자다.
    /// T = State<T> 일때 State<T>.Data를 T에 넣는다.
    /// State<T> = T 일때 new State<T>(value)를 생성한다.
    //public static implicit operator T(State<T> value) => value.Data is not null ? (T)Convert.ChangeType(value.Data, typeof(T)) : throw new Exception(nameof(value));
    public static implicit operator State<T>(T value) => new State<T>(value);
    /// (T)Convert.ChangeType(value.Data, typeof(T))를 써야하는 이유
    /// 기본 데이터 형식을 다른 데이터 형식으로 변환 
    /// 5가지 결과중 하나를 생성한다.
    /// 1. 변환이 없는 경우 => 원래 형식의 인스턴스만 반환 
    /// 2. 변환이 실패한 경우 => throw Exception
    /// 3. 변환이 성공한 경우 => 변환된 인스턴스 반환 
    /// 4. overflow
    /// 5. formatException
    /// Constructor에서 T 타입의 param을 넣었는데, 굳이 Convert.ChangeType에서 throw Exception을 할 이유?
    public static implicit operator T(State<T> value) => value.Data is not null ? value.Data : throw new Exception(nameof(value));
    public int GetValidateCount() => validateCount;
    public FlagState GetCurrentFlag() => flag;
}
public class Behaviour
{
    
    public event EventHandler<EventArgs> behaviour;

    public void AddBehaviour(Action<object, EventArgs> e)
    {
        if(e is not null)
        {
            EventHandler<EventArgs> h = e.Invoke!;
            behaviour += h;
        }
        else
        {
            throw new Exception(nameof(e));
        }
    }

    public void CallBehaviour(EventArgs e)
    {
        if(e is not null && behaviour is not null)
        {
            EventHandler<EventArgs> temp = behaviour;
            temp.Invoke(this, e);
        }
    }
}
public class Compound<T>
{
    public State<T> state { get; private set;}
    /// static delete
    public static Behaviour behaviour { get; private set;}

    static Compound()
    {
        behaviour = new Behaviour();
    }
    public Compound(T input)
    {
        state = new State<T>(input);
    }

    public void OnAddresssFunction(Action<object, EventArgs> e)
    {
        behaviour.AddBehaviour(e);
    }

    public void OnCallBehaviour()
    {
        behaviour.CallBehaviour(state);
    }
}




/// 파일명, 경로, 함수명, 라인 넘버등 로깅 이벤트 발생시점의 정보가 자세하게 담겨 있어야 한다.
/// 파일명?     따로 저장안하는데
/// 경로?       마찬가지
/// 함수명?     이건 등록해야겠다.
/// 시간?       이것도
/// 라인넘버?   이것도  
public class Logger<T>
{
    #region Logger Configuration Components
    public struct Component<T>
    {
        public string MethodName = string.Empty;
        public DateTime InvokeTime = DateTime.MinValue;
        public int LineNumber = 0;

        /// Compound의 event 감지가 필요
        public Compound<T> target;
        public Component(Compound<T> target) 
        {
            this.target = target;
        }
        public Component(T input)
        { 
            target = new Compound<T>(input);
        }
    };
    List<Component<T>> components = new List<Component<T>>();    
    #endregion
    
    LoggerStream loggerStream = new LoggerStream(
        folderPath: @"D:\최상위 루트 코드파일\repos\VSCode\",
        folderName: @"LoggerStream",
        fileName: "Sample",
        fileForm: FileForm.TXT);


    /// Component 추가
    /// Task 사용하여 실행 도중에 추가가 진행될 수 있게끔 Thread를 조절
    public void Add() => components.Add(Configuration());

    /// Report가 추가된 Component 생성
    public Component<T> Configuration()
    {
        
        return new Component<T>();
    }   

    /// Report 작성
    /// 양식 정할것
    /// Component parameter add
    public StringBuilder Report(string _classInfo, string _methodInfo, string _componentInfo)
    {
        StringBuilder builder = new StringBuilder();
        string format = "\n" +
            "ClassName : {0}\n" +
            "└─ MethodName : {1}\n" +
            "└─ Component : {2}\n";

        builder.AppendFormat(format, _classInfo, _methodInfo, _componentInfo);
        return builder;
    }

    public void Read()
    {
        using (StreamReader reader = new StreamReader(loggerStream.file_FullPath))
        {
            try
            {
                string line = String.Empty;
                line = reader.ReadLine()!;
                while (line != null)
                {
                    WriteLine(line);
                    line = reader.ReadLine()!;
                }
                reader.Close();
            }
            catch (Exception e) { WriteLine("Exception: " + e.Message); }
            finally { WriteLine("Executing finally block."); }
        }
    }

    public void Write(StringBuilder builder)
    {
        if (!Directory.Exists(loggerStream.folder_FullPath))
        {
            loggerStream.CreateFolder();
            if (!File.Exists(loggerStream.file_FullPath))
            {
                loggerStream.CreateFile();
            }
        }

        using (StreamWriter writer = new StreamWriter(loggerStream.file_FullPath, 
                                        true, 
                                        Encoding.Unicode))        
        {
            WriteLine(builder.ToString());
        }
    }
}
public enum FileForm
{
    NONE = 0,
    TXT = 1
}
public class LoggerStream
{
    private string folderPath = String.Empty;
    private string folderName = String.Empty;
    public string folder_FullPath { get; private set; }

    private string filePath = String.Empty;
    private string fileName = String.Empty;
    private FileForm fileForm = FileForm.NONE;
    public string file_FullPath { get; private set; }

    public LoggerStream(string folderPath, string folderName, string fileName, FileForm fileForm)
    {
        this.folderPath = folderPath;
        this.folderName = folderName;
        
        this.fileName = fileName;
        this.fileForm = fileForm;

        CompletePaths(fileForm);
    }
    internal void CompletePaths(FileForm form)
    {
        folder_FullPath = CombinePaths(folderPath, folderName);
        filePath = CombinePaths(folder_FullPath, fileName);
        file_FullPath = filePath + ReturnForm(form);
    }
    internal string CombinePaths(string pre, string post)
    {
        return Path.Combine(pre, post);
    }
    internal string ReturnForm(FileForm target) => target switch
    {
        FileForm.TXT => @".txt",
        _ => throw new Exception()
    };


    public void CreateFile()
    {
        FileStream fileStream = File.Create(file_FullPath);
        fileStream.Close();
    }
    public void CreateFolder()
    {
        Directory.CreateDirectory(folder_FullPath);
    }
}
