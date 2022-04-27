using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace calc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<double> nums = new List<double>();//массив операндов
        List<string> ops = new List<string>();//массив операторов
        string cur_num = "";//текущее число
        bool is_neg = false;//является ли число отрицательным

        bool is_symbol(string pressed)
        {
            return pressed == "+" || pressed == "-" || pressed == "/" || pressed == "*";
        }
        
        public void btn_clear_press(object sender,RoutedEventArgs e)
        {//очистка содержимого полей и массивов
            lbl.Content = "";
            lbl_str.Content = "";
            cur_num = "";
            nums.Clear();
            ops.Clear();
        }
        public void btn_rmv_press(object sender, RoutedEventArgs e)
        {
            string res = lbl.Content.ToString();
            lbl.Content = (res.Length < 2) ? "" : res.Substring(0, res.Length - 1);//удаление символа в поле
            //если удаляется символ операции, убираем его из списка ops
            if (is_symbol(res.Substring(res.Length - 1)))
            {
                ops.RemoveAt(ops.Count - 1);
                cur_num = nums[nums.Count - 1].ToString();//перезаписываем текущее число, чтобы была возможность его дополнить при необходимости
                nums.RemoveAt(nums.Count - 1);
            }
            else
            {
                if (cur_num.Length < 2)
                {//если число состоит из 1 цифры, текущее число обнуляем и его же удаляем из списка nums
                    cur_num = "";
                    nums.RemoveAt(nums.Count - 1);
                    is_neg = false;
                }//иначе удаляем посленюю цифру числа
                else cur_num = cur_num.Substring(0, cur_num.Length - 1);
            }
        }
        public void btn_num_press(object sender, RoutedEventArgs e)
        {//обработчик нажатия на цифры и ","
            Button pressed = (Button)sender;
            if (pressed.Content.ToString() == "," && cur_num.IndexOf(',') != -1) return;//исключаем повторное введение "," в числе
            if (pressed.Content.ToString() == "0" && lbl.Content.ToString() == "") return;//исключаем введение нуля при пустом поле
            cur_num += pressed.Content.ToString();//дополняем текущее число и поле
            lbl.Content += pressed.Content.ToString();
        }
        public void btn_pcn_press(object sender, RoutedEventArgs e)
        {//нажатие на "%"
            string res = lbl.Content.ToString();
            if (res != "" && !is_symbol(res.Substring(res.Length - 1)))
            {//не выполняется при пустом поле и после знаков операций
                int ind = res.LastIndexOf(cur_num);
                cur_num = (Convert.ToDouble(cur_num) / 100).ToString();//переводим число в проценты
                res = res.Substring(0, ind);
                res += cur_num;
                lbl.Content = res;
            }
        }
        public void btn_otr_press(object sender, RoutedEventArgs e)
        {//нажатие на "+/-"
            string res = lbl.Content.ToString();
            if (is_neg)
            {//если число уже отрицательное
                res = res.Substring(0, res.LastIndexOf(cur_num));
                cur_num = cur_num.Substring(cur_num.LastIndexOf("-") + 1);//убираем минус перед ним
                res += cur_num;
                lbl.Content = res;
                is_neg = false;
            }
            else
            {
                if (res == "" || is_symbol(res.Substring(res.Length - 1)))
                {//при пустом поле или после знака операции ставим минус до числа
                    cur_num += "-";
                    lbl.Content += "-";
                }
                else
                {//иначе заменяем текущее число его отрицанием
                    int ind = res.LastIndexOf(cur_num);
                    cur_num = "-" + cur_num;
                    res = res.Substring(0, ind);
                    res += cur_num;
                    lbl.Content = res;
                }
                is_neg = true;
            }
        }
        public void btn_op_press(object sender, RoutedEventArgs e)
        {//обработчик операций
            string res = lbl.Content.ToString();
            Button pressed = (Button)sender;
            if (res == "") return;//исключаем введение при пустом поле
            if (is_symbol(res.Substring(res.Length - 1)))
            {//если уже введен знак операции
                if (is_neg) return;//при конструкции вида "число операция -" не меняем знак отрицания
                res = res.Substring(0, res.Length - 1);//заменяем предыдущую операцию на нововеденную
                ops.RemoveAt(ops.Count - 1);
            }//иначе добавляем число до знака операции в массив nums
            else nums.Add(Convert.ToDouble(cur_num));
            ops.Add(pressed.Content.ToString());//добавляем саму операцию в массив ops
            res += pressed.Content.ToString();
            lbl.Content = res;
            cur_num = "";//обнуляем текущее число
            is_neg = false;
        }
        public void btn_eq_press(object sender, RoutedEventArgs e)
        {//нажатие на "="
            string res = lbl.Content.ToString();
            if (res != "" && !is_symbol(res.Substring(res.Length - 1)))
            {//срабатывает только если предыдущий символ - число
                nums.Add(Convert.ToDouble(cur_num));//добавляем число до знака = в массив
                solve();//находим решение
                nums.Clear();//очищаем список чисел
                lbl_str.Content = lbl.Content + "=" + cur_num;//фоном выводим историю операции
                lbl.Content = cur_num;//в рабочем поле выводим ответ
            }
        }
        void solve()
        {
            if (nums.Count == ops.Count) ops.RemoveAt(ops.Count - 1);//если последним был введен знак операции, удаляем его
            while (ops.Count > 0)
            {
                int ind = ops.IndexOf("/");
                while (ind != -1)
                {//находим знак "/" в выражении и заменяем на "* 1/число"
                    ops[ind] = "*";
                    nums[ind + 1] = 1 / nums[ind + 1];
                    ind = ops.IndexOf("/");
                }
                ind = ops.IndexOf("*");
                while (ind != -1)
                {//первым в приоритете выполняем умножение
                    nums[ind] = nums[ind] * nums[ind + 1];
                    ops.RemoveAt(ind);
                    nums.RemoveAt(ind + 1);
                    ind = ops.IndexOf("*");
                }
                for (int i = 0; i < ops.Count; i++)
                {//выполняем сложение и вычитание, пока массив ops не опустеет
                    nums[i] = (ops[i] == "+") ? nums[i] + nums[i + 1] : nums[i] - nums[i + 1];
                    ops.RemoveAt(i);
                    nums.RemoveAt(i + 1);
                }
            }
            cur_num = nums[0].ToString();
        }
    }
}
