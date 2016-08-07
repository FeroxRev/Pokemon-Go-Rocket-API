// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the ENCRYPT_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// ENCRYPT_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ENCRYPT_EXPORTS
#define ENCRYPT_API __declspec(dllexport)
#else
#define ENCRYPT_API __declspec(dllimport)
#endif

extern "C" ENCRYPT_API int __stdcall encrypt(const unsigned char *input, size_t input_size,
		const unsigned char* iv, size_t iv_size,
		unsigned char* output, size_t * output_size);
