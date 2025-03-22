#ifndef __SOL_H__
#define __SOL_H__

#include <cstdint>
#include <string>
#include <vector>
#include <map>
#include <variant>
#include <stdexcept>
#include <type_traits>

namespace sol
{
    struct SolValue;

    using SolNull = std::nullptr_t;
    using SolBoolean = bool;
    using SolInteger = int32_t;
    using SolDouble = double;
    using SolString = std::string;
    using SolArray = std::vector<SolValue>;
    using SolObject = std::map<std::string, SolValue>;
    using SolXml = SolString;
    using SolBinary = std::vector<uint8_t>;


    enum class SolType : uint8_t
    {
        Null = 0x01,
        BooleanFalse = 0x02,
        BooleanTrue = 0x03,
        Integer = 0x04,
        Double = 0x05,
        String = 0x06,
        Array = 0x09,
        Object = 0x0A,
        Xml = 0x0B,
        Binary = 0x0C,
    };


    struct SolValue
    {
        SolType type;
        std::variant<SolNull, SolBoolean, SolInteger, SolDouble, SolString, SolArray, SolObject, SolBinary> value;

        SolValue(SolNull = nullptr) : type(SolType::Null), value(nullptr) {}
        SolValue(SolBoolean v) : type(v ? SolType::BooleanTrue : SolType::BooleanFalse), value(v) {}
        SolValue(SolInteger v) : type(SolType::Integer), value(v) {}
        SolValue(SolDouble v) : type(SolType::Double), value(v) {}
        SolValue(const SolString& v, bool isXml = false) : type(isXml ? SolType::Xml : SolType::String), value(v) {}
        SolValue(const char* v) : SolValue(SolString(v)) {}
        SolValue(const SolArray& v) : type(SolType::Array), value(v) {}
        SolValue(const SolObject& v) : type(SolType::Object), value(v) {}
        SolValue(const SolBinary& v) : type(SolType::Binary), value(v) {}
        SolValue(const SolValue& v) : type(v.type), value(v.value) {}

        template <typename T>
        SolValue(SolType t, const T& v) : type(t), value(v) {}

        template <typename T>
        bool is() const
        {
            switch (type)
            {
            case SolType::Null:
                return std::is_same_v<T, SolNull>;
            case SolType::BooleanFalse:
            case SolType::BooleanTrue:
                return std::is_same_v<T, SolBoolean>;
            case SolType::Integer:
                return std::is_same_v<T, SolInteger>;
            case SolType::Double:
                return std::is_same_v<T, SolDouble>;
            case SolType::String:
            case SolType::Xml:
                return std::is_same_v<T, SolString>;
            case SolType::Array:
                return std::is_same_v<T, SolArray>;
            case SolType::Object:
                return std::is_same_v<T, SolObject>;
            case SolType::Binary:
                return std::is_same_v<T, SolBinary>;
            default:
                return false;
            }
        }

        template <typename T>
        T get() const
        {
            switch (type)
            {
            case SolType::Null:
                return T();
            case SolType::BooleanFalse:
                return T(false);
            case SolType::BooleanTrue:
                return T(true);
            case SolType::Integer:
                return std::get<SolInteger>(value);
            case SolType::Double:
                return std::get<SolDouble>(value);
            case SolType::String:
            case SolType::Xml:
                return std::get<SolString>(value);
            case SolType::Array:
                return std::get<SolArray>(value);
            case SolType::Object:
                return std::get<SolObject>(value);
            case SolType::Binary:
                return std::get<SolBinary>(value);
            default:
                throw std::runtime_error("Type mismatch");
            }
        }
    };


    struct SolFile
    {
        std::string path;
        std::string solname;
        std::string errmsg;
        SolObject data;

        bool valid() const { return errmsg.empty(); }
    };


    bool IsKnownType(SolType type);

    bool ReadSolFile(SolFile& file);

    SolInteger ReadSolInteger(uint8_t* data, int size, int& index, bool unsign = false);

    SolDouble ReadSolDouble(uint8_t* data, int size, int& index);

    SolString ReadSolString(uint8_t* data, int size, int& index, std::vector<std::string>& strpool, bool add2pool = true);

    SolXml ReadSolXml(uint8_t* data, int size, int& index, std::vector<std::string>& strpool);

    SolBinary ReadSolBinary(uint8_t* data, int size, int& index);
}

#endif // !__SOL_H__
