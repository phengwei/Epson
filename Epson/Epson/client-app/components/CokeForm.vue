<template>
  <div class="absolute mx-20 sm:mx-2 inset-0 flex flex-col justify-start items-center">
    <div
      class="
        h1
        font-black
        flex
        mt-6
        xl:mt-24
        items-center
        text-2xl
        md:text-3xl
        2xl:text-4xl
      "
    >
      Redeem now
    </div>
    <div class="h1 font-bold flex 
    my-1 
    xl:mt-2 2xl:mt-6 
    xl:mb-6 2xl:mb-10
    items-center">
      Fill in details below
    </div>

    <div class="flex flex-wrap mb-6 w-full 
    px-18 2xl:px-24 ">
      <div class="w-full px-3 xl:pb-2 2xl:pb-4">
        <label
          class="block tracking-wide font-bold mb-2"
          :class="{ ' text-red-600': errorlist.includes('fullname') }"
          for="grid-fullname"
        >
          Full Name
        </label>
        <input
          id="grid-fullname"
          v-model="fullname"
          type="text"
          placeholder=""
          class="
            appearance-none
            block
            w-full
            bg-gray-200
            border border-gray-200
            rounded-lg
            py-3
            px-4
            mb-3
            leading-tight
            focus:outline-none focus:bg-white focus:border-gray-500
          "
          
          @focus="popErrors(['fullname'])"
        />
      </div>



      <div class="w-full px-3 xl:pb-2 2xl:pb-4">
        <label
          class="block tracking-wide font-bold mb-2"
          :class="{ ' text-red-600': errorlist.includes('email') }"
          for="grid-email"
        >
          Email
        </label>
        <input
          id="grid-email"
          v-model="email"
          type="email"
          class="
            appearance-none
            block
            w-full
            bg-gray-200
            border border-gray-200
            rounded-lg
            py-3
            px-4
            mb-3
            leading-tight
            focus:outline-none focus:bg-white focus:border-gray-500
          "
          
          @focus="popErrors(['email'])"
        />
      </div>







      <div class="w-full px-3 xl:pb-2 2xl:pb-4">
        <label class="block tracking-wide font-bold mb-2" for="grid-mobile"  :class="{ ' text-red-600': errorlist.includes('mobile') }">
          Mobile
        </label>
        <input
          id="grid-mobile"
          v-model="mobile"
          type="text"
          class="
            appearance-none
            block
            w-full
            bg-gray-200
            border border-gray-200
            rounded-lg
            py-3
            px-4
            mb-3
            leading-tight
            focus:outline-none focus:bg-white focus:border-gray-500
          "

          
          @focus="popErrors(['mobile'])"
        />
      </div>

      <div class="w-full px-3 xl:pb-2 2xl:pb-4">
        <label class="block tracking-wide font-bold mb-2" for="grid-postcode"  :class="{ ' text-red-600': errorlist.includes('postcode') }">
          Postcode
        </label>
        <input
          id="grid-postcode"
          
          v-model="postcode"
          type="text"
          class="
            appearance-none
            block
            w-full
            bg-gray-200
            border border-gray-200
            rounded-lg
            py-3
            px-4
            mb-3
            leading-tight
            focus:outline-none focus:bg-white focus:border-gray-500
          "
          
          @focus="popErrors(['postcode'])"
        />
      </div>

      <div class="w-full px-3 pb-4">
        <label class="block tracking-wide font-bold mb-2" for="grid-upload" :class="{ ' text-red-600': errorlist.includes('fileupload') }"  >
          Upload Receipt
        </label>
        <div class="flex flex-row justify-end">
          <input
            id="grid-fileobj"
            v-model="fileName"
            type="text"
            class="
              appearance-none
              block
              w-full
              bg-gray-200
              border border-gray-200
              rounded-l-lg
              py-3
              px-4
              leading-tight
              focus:outline-none focus:bg-white focus:border-gray-500
            "
          />
          <label
            class="
              w-64
              flex flex-row
              justify-around
              items-center
              border border-red-600
              hover:border-red-700
              py-3
              rounded-r-lg
              tracking-wide
              cursor-pointer
              hover:bg-red-700
              text-white
              bg-red-600
              ease-linear
              transition-all
              duration-150
            " 
            
          >
            <fa :icon="['fas', 'upload']"></fa>
            <span class="text-base leading-normal">Choose file</span>
            <input
              id="grid-upload"
              type="file"
              name="receipt"
              class="hidden"
              accept="application/pdf, image/*"
              @change="
                filesChange($event.target.name, $event.target.files)
                fileCount = $event.target.files.length
              "
               @focus="popErrors(['fileupload'])"
            />
          </label>
        </div>

        <p class="mt-2  text-sm sm:text-base">
          JPG / PNG / PDF. Maximum file size 32MB. <br />Kindly ensure that the
          receipt attached is clear and ligible.
        </p>
      </div>

      <div class="w-full px-3 py-2 2xl:py-8 text-sm sm:text-base">
        <label class="inline-flex items-center">
          <input
           v-model="tncchecked"
            class="
              text-red-500
              w-8
              h-8
              mr-2
              focus:ring-red-400 focus:ring-opacity-25
              border border-gray-300
              rounded
            "
            type="checkbox"
            
          />
          <p>
          I confirm that I have read, consent and agree to campaign <a class="text-red-600 cursor-pointer underline" @click="openModal">Terms &amp; Conditions</a>, and I am above 18 years of age.
          </p>
         
        </label>
      </div>

      <div class="w-full px-3   text-sm sm:text-base">
        <label class="inline-flex items-center">
          <input
           v-model="tnc2checked"
            class="
              text-red-500
              w-8
              h-8
              mr-2
              focus:ring-red-400 focus:ring-opacity-25
              border border-gray-300
              rounded
            "
            type="checkbox"
            
          />
          <p>
          Check this box to subscribe to future promotions notification  </p>
         
        </label>
      </div>



      <div class="w-full px-3 py-3 md:py-6 xl:py-2 2xl:py-10 text-center">
        <button
          class="
            md:w-96
            font-bold
            bg-red-600
            text-white
            px-28
            md:px-32
            py-4
            rounded-full
          "
          
            :disabled="isLoading"
           @click="onSubmit"
        >
          Submit

          
        <svg
          v-show="isLoading"
          class="inline-block animate-spin h-8 w-8 text-white"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg> 
        </button>

      </div>
    </div>
  </div>
</template>

<script>
export default {

  data() {
    return {
      errorlist: [],

      errordesc: {
        fullname: 'Full name is required.',
        mobile: 'Mobile number is required.',
        postcode: 'Postcode is required.',
        email: 'Email is required.',
        fileupload: 'Receipt file is required.',
        tnc: 'Please agree to Terms and Conditions',
      },

      uploadedFiles: [],

      fileName: '',
isLoading: false,
      fullname: '',
      postcode: '',
      mobile: '',
      email: '',
      tncchecked: false,
      tnc2checked: false,

      fileCount: 0,
    }
  },
  methods: {
    openModal(){
      this.$parent.openModal();
    },
    onFileChange(event) {
      if (event != null && event.target != null) {
        const fileData = event.target.files[0]
        if (fileData) this.fileName = fileData.name
      }
    },

    validEmail(email) {
      const re =
        /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
      return re.test(email)
    },

    async onSubmit(e) {

      this.isLoading = true;
      const isValid = this.checkForm(e)

      if (!isValid) {
        const errors = []
        for (let i = 0; i < this.errorlist.length; i++) {
          const err = this.errorlist[i]
          const errdesc = this.errordesc[err]
          if (errdesc != null) {
            errors.push(errdesc)
          }
        }
          this.$swal('Unable to proceed!', errors.join('<br/>'), 'error')
          .then((result) => {
                this.isLoading = false;
            });

        return false;
      }
      const formaction = "submitcontest";
      let payload = {
        fullname: this.fullname,
        postcode: this.postcode,
        mobile: this.mobile,
        email: this.email,
        action: formaction,
      }


      // Wait until recaptcha has been loaded.
      // await this.$recaptchaLoaded();


      const token = await this.$recaptcha.execute('submitcontest')
      payload.token = token
      try {
        // const source = this.$axios.CancelToken.source();
        // const endpoint = `${this.$config.restUrl}/submitcnygtchannel`
        const endpoint = `${this.$config.restUrl}/coke/v1/submitcontest`
        payload.notifyemail = 'hoongtat@lavishteam.com';

        payload = JSON.stringify(payload)
        const formData = new FormData()

        for (let ii = 0; ii < this.uploadedFiles.length; ii++) {
          const item = this.uploadedFiles[ii]
          formData.append('receipt', item.file, item.name)
        }

        formData.append('fullname', this.fullname);
        formData.append('postcode', this.postcode);
        formData.append('mobile', this.mobile);
        formData.append('email', this.email);
        formData.append('action', formaction);

        formData.append('g-recaptcha-response', token);
        formData.append("body", JSON.stringify(payload));



        await this.$axios
          .post(endpoint, formData, {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          })
          .then((res) => {
            console.log('res', res)
            if (res && res.data) {
              if (res.data.code === 200) {
                this.$swal(
                  'Congratulation!',
                  'Your application has been submitted successfully.',
                  'success'
                ).then((result) => {
                  window.location.reload();
                })

                
              } else {

                            
                    this.$swal('Ops! Sorry', 'Unable to submit', 'error')
                    .then((result) => {
                            this.isLoading = false;
                        });

               
              }
            }
          })
      } catch (error) {
          this.$swal('Ops! Sorry', 'Unable to submit, error:'+error, 'error')
        .then((result) => {
                this.isLoading = false;
            });

      }
    },

    checkForm(e) {
      this.errors = []

      if (!this.fullname || this.fullname.trim() === '') {
        this.pushError('fullname')
      } else {
        this.popErrors(['fullname'])
      }

      if (!this.postcode || this.postcode.trim() === '') {
        this.pushError('postcode')
      } else {
        this.popErrors(['postcode'])
      }

      if (!this.mobile || this.mobile.trim() === '') {
        this.pushError('mobile')
      } else {
        this.popErrors(['mobile'])
      }

      if (!this.email || this.email.trim() === '') {
        this.pushError('email')
      } else {
        this.popErrors(['email'])
      }

      if (this.fileCount <= 0) {
        this.pushError('fileupload')
      } else {
        this.popErrors(['fileupload'])
      }

      if (!this.tncchecked) {
        this.pushError('tnc')
      } else {
        this.popErrors(['tnc'])
      }

 
      if (!this.errorlist.length) {
        return true
      }

        

      e.preventDefault()
    },
    
     async mounted() {
          try {
            await this.$recaptcha.init()
          } catch (e) {
            console.error(e);
          }
    },
    beforeDestroy() {
          this.$recaptcha.destroy()
    },

     pushError(key){
            const index = this.errorlist.indexOf(key);
            if (index < 0) {
                this.errorlist.push(key);
            }
         },
    popErrors(keys) {
      for (let i = 0; i < keys.length; i++) {
        const key = keys[i]
        const index = this.errorlist.indexOf(key)
        if (index >= 0) {
          this.errorlist.splice(index, 1)
        }
      }
    },
    filesChange(fieldName, fileList) {
      // handle file changes
      if (!fileList.length) return
      Array.from(Array(fileList.length).keys())
        // eslint-disable-next-line array-callback-return
        .map((x) => {
          this.uploadedFiles.push({
            // eslint-disable-next-line object-shorthand
            fieldName: fieldName,
            file: fileList[x],
            name: fileList[x].name,
          })

          this.fileName = fileList[x].name;
          
            this.popErrors(['fileupload'])
        })
         
    },
  },
}
</script>
