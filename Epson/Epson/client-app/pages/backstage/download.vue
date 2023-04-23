<template>
    <div>
        <img :src="blobData" >
    </div>
</template>

<script>
import { mapGetters } from 'vuex'

export default {
  name: 'dashboard-main',
  middleware: 'auth',
  computed: {
    ...mapGetters(['loggedInUser'])
  },
  
  data() {
    return {
        blobData: null,
        records: {},
        pageSize: 5,
        pageNumber: 1
    }
  },
  methods:{
      
      downloadFile(fileKey){
        this.$axios({
                    url: `${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`,
                    method: 'GET',
                    responseType: 'blob',
        })
        .then((response) => {
              const reader = new FileReader();
              reader.readAsDataURL(response.data); 
              reader.onload = () => {
                  this.blobData = reader.result;
              }

                    /** 
                     const fileURL = window.URL.createObjectURL(new Blob([response.data]));
                     const fileLink = document.createElement('a');
   
                     fileLink.href = fileURL;
                     fileLink.setAttribute('download', fileKey);
                     document.body.appendChild(fileLink);
   
                     // fileLink.click();
                     **/
                });
      },
       displayFile(fileKey){
        console.log(`==${fileKey}`);
        this.$axios({
                    url: `${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`,
                    method: 'GET',
                    responseType: 'blob',
        })
        // this.$axios.get(`${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`)
        .then((response) => {
                     const fileURL = window.URL.createObjectURL(new Blob([response.data]));

                     window.open(fileURL);

                     /** 
                     const fileLink = document.createElement('a');
   
                     fileLink.href = fileURL;
                     fileLink.setAttribute('download', fileKey);
                     document.body.appendChild(fileLink);
   
                     fileLink.click();
                     **/
                });
      }
  },
   mounted() {
    console.log(this.$route.query.fileKey);
    const fileKey = this.$route.query.fileKey;
    if(fileKey != null && fileKey !== ""){
        this.downloadFile(fileKey);     
    }else{
        console.log("no file ket provided");
    }
       // this.$axios.get(`${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`)
       // .then(response => {
       //     this.records = response.data;
       // })
       // petrol/c4ce0946-2b29-4432-a637-f35cf4f54ba8.jpg
       /**
        this.$axios.get(`${this.$config.restUrl}/api/contests?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}`)
        .then(response => {
            this.records = response.data;
        })
        */
    }

}
</script>
