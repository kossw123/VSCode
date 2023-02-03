using System.Text;
using System.IO;
using static System.Console;

//MainClass<int> main = new MainClass<int>(123123);

Logger logger = new Logger();

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
    public static implicit operator State<T>(T value) => new State<T>(value);
    public static implicit operator T(State<T> value) => value.Data is not null ? (T)Convert.ChangeType(value.Data, typeof(T)) : throw new Exception(nameof(value));

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
public class CustomBehaviour
{
    public delegate void CustomEventHandler(object sender, EventArgs e);
    public event CustomEventHandler? customHandler;
    public void AddBehaviour(Action<object, EventArgs> e)
    {
        if(e is not null)
        {
            CustomEventHandler temp = e.Invoke!;
            customHandler += temp;
        }
        else
        {
            throw new Exception(nameof(e));
        }
    }

    public void CallBehaviour(EventArgs e)
    {
        if(e is not null && customHandler is not null)
        {
            CustomEventHandler temp = customHandler;
            temp?.Invoke(this, e);
        }
    }
}
public class Compound<T>
{
    public State<T> state { get; private set;}
    public static Behaviour behaviour { get; private set;}
    public static CustomBehaviour customBehaviour { get; private set; }

    static Compound()
    {
        behaviour = new Behaviour();
        customBehaviour = new CustomBehaviour();
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

    public void OnAddresssCustomFunction(Action<object, EventArgs> e)
    {
        customBehaviour.AddBehaviour(e);
    }

    public void OnCallCustomBehaviour()
    {
        customBehaviour.CallBehaviour(state);
    }
}




/// 파일명, 경로, 함수명, 라인 넘버등 로깅 이벤트 발생시점의 정보가 자세하게 담겨 있어야 한다.
/// 파일명?     따로 저장안하는데
/// 경로?       마찬가지
/// 함수명?     이건 등록해야겠다.
/// 시간?       이것도
/// 라인넘버?   이것도

/// 근데 어디에다가 등록해서 가지고 있을까?
public class Logger
{
    #region Logger Components
    public struct Component
    {
        public string MethodName = string.Empty;
        public DateTime InvokeTime = DateTime.MinValue;
        public int LineNumber = 0;
        public Component() {}
    };
    List<Component> components = new List<Component>();    
    #endregion
    
#region Configuration
    public readonly string fileName;
    public readonly string filePath = @"D:\최상위 루트 코드파일\repos\VSCode\Sample.txt" ;
    StreamReader reader;
    StreamWriter writer;
    StringBuilder builder;
#endregion


    public Logger()
    {
        
    }
    public void Read()
    {
        using (StreamReader reader = new StreamReader(filePath))
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
        using(StreamWriter writer = new StreamWriter(filePath, true, Encoding.Unicode))
        {
            WriteLine(builder.ToString());
        }
    }
    private string NowTime() => DateTime.Now.ToString("yyyy/MM/dd/hh/ss");
}