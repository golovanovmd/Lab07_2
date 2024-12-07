using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using AnimalLibrary;

public class Program
{
    /*объявляется статический метод GetClasses, который принимает строку с именем пространства имен 
  * nameSpace и возвращает перечисление (IEnumerable) типов, находящихся в указанном пространстве имен*/
    static IEnumerable<Type> GetClasses(string nameSpace)
    {
        var asm = Assembly.Load(nameSpace);
        //создается переменная asm, в которой мы вызываем метод Assembly.Load для загрузки сборки с указанным именем пространства имен.
        return asm.GetTypes()//мы вызываем метод GetTypes() для получения всех типов в этой сборке.
                  .Where(type => type.Namespace == nameSpace);
    }//метод расширения Where, чтобы отфильтровать только те типы, чье пространство имен совпадает с указанным пространством имен.

    public static void Main(string[] args)
    {//создается экземпляр класса StreamWriter для записи данных в файл MyLibrary.xml.
        using (StreamWriter fout = new StreamWriter(@"..\..\..\..\MyLibrary.xml"))//создание файла
        {//мы используем оператор using для создания объекта StreamWriter с именем fout и указываем путь к файлу "MyLibrary.xml"
            fout.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            // Затем мы записываем строки <Library> и <name>ClassLibrary1</name>, чтобы начать создание XML-файла.
            fout.WriteLine("<Library>");//В файл записываются первые три строки с указанием версии XML и открывается корневой элемент Library.
            fout.WriteLine("<name>ClassLibrary1</name>");

            foreach (var myType in GetClasses("AnimalLibrary"))
            {//Внутри цикла выводится информация о типе в консоль, а также записывается в файл элемент type с информацией о типе, его модификаторах и базовом типе.
                Console.WriteLine("typename=" + myType.Name + ":");
                //- myType.Name возвращает имя типа.
                //Метод IsAbstract возвращает true, если тип является абстрактным
                Console.WriteLine("abstract=" + myType.IsAbstract + ";");
                Console.WriteLine("public=" + myType.IsPublic + ";");
                // myType.BaseType.Name возвращает имя базового типа данного типа.
                Console.WriteLine("basetype=" + myType.BaseType.Name + ";");
                fout.WriteLine("\t<type>");
                fout.WriteLine("\t\t<name>" + myType.Name + "</name>");
                //Затем мы записываем начало элемента <type> в XML-файл.
                if (myType.IsPublic || myType.IsAbstract)
                {/*Если тип является публичным или абстрактным, мы записываем начало элемента <modifiers> и 
              * дополнительно записываем "public" или "abstract" в зависимости от того, какой модификатор присутствует*/
                    fout.Write("\t\t<modifiers>");
                    //В конце, если модификаторы присутствуют, закрываем элемент <modifiers>
                    if (myType.IsPublic) fout.Write("public ");
                    if (myType.IsAbstract) fout.Write("abstract");

                    fout.WriteLine("</modifiers>");
                }
                fout.WriteLine("\t\t<basetype>" + myType.BaseType.Name + "</basetype>");

                foreach (MemberInfo member in myType.GetMembers(BindingFlags.DeclaredOnly
                    | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
                {/* происходит цикл по всем членам типа (member), полученным с использованием метода GetMembers и 
              * заданными флагами, указывающими на необходимость получения только объявленных членов, экземпляров, 
              * публичных и статических членов.*/
                    fout.WriteLine("\t\t<member>");
                    fout.WriteLine("\t\t\t<name>" + member.Name + "</name>");
                    fout.WriteLine("\t<membertype>" + member.MemberType + "</membertype>");
                    Console.WriteLine("\tname=" + member.Name);
                    Console.WriteLine("\tmembertype=" + member.MemberType);
                    /*Внутри цикла выводится информация о каждом члене в консоль, а также записывается в файл 
                     * соответствующий элемент с информацией о его имени, типе и специфической информации для полей и свойств*/
                    if (member.MemberType == MemberTypes.Field)
                    {//проверяется, явл ли тип члена MemberType равным MemberTypes.Field. Он относится к членам, которые являются полями (variables) в классе.
                        FieldInfo field = myType.GetField(member.Name);
                        //Здесь создается переменная field типа FieldInfo, которая содержит информацию о поле с именем
                        //Здесь проверяется, была ли успешно найдена информация о поле (не равна ли field значению null)
                        if (field != null)
                        {
                            Type fieldType = field.FieldType;
                            //\t\t используется для добавления отступа (табуляции) в выводе. fieldType.Name возвращает имя типа поля.
                            Console.WriteLine("\t\ttype=" + fieldType.Name);
                            fout.WriteLine("\t\t\t<fieldtype>" + fieldType.Name + "</fieldtype>");
                        }//объект класса StreamWriter. <fieldtype> и </fieldtype> являются открывающим и закрывающим тегами для помещения информации о типе поля.
                    }

                    if (member.MemberType == MemberTypes.Property)
                    {//В этой строке проверяется, является ли тип члена MemberType равным MemberTypes.Property
                        PropertyInfo prop = myType.GetProperty(member.Name);
                        if (prop != null)
                        {
                            Type propertyType = prop.PropertyType;
                            Console.WriteLine("\t\ttype=" + propertyType.Name);
                            fout.WriteLine("\t\t\t<propertytype>" + propertyType.Name + "</propertytype>");
                        }
                    }

                    fout.WriteLine("\t\t</member>");
                }//После завершения цикла по членам типа закрывается элемент type и происходит переход к следующему типу.
                Console.WriteLine();
                fout.WriteLine("\t</type>");
            }//По окончании цикла по типам записывается закрывающий элемент Library и закрывается файл StreamWriter.
            fout.WriteLine("</Library>");
        }
    }
}