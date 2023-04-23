<template>
  <section class="section">
    <div class="w-full px-3">
      <div class="content">
        
        
      </div>
      <div class="bg-gray-200 px-3">
          <div class="flex flex-row  justify-center items-center">
              <p>
            <strong>{{ loggedInUser.username }}</strong>
            
            
            </p>
            <tailable-pagination class="flex-grow" @page-changed="pageChanged"
                :data="records"
                :showNumbers="true">
            </tailable-pagination>

            <div class="flex justify-center items-center px-1 text-sm">
                <button @click="downloadExcel">Download Report</button>
            </div>
            <div class="flex justify-center items-center text-sm">
                <button @click="logout">logout</button>
            </div>
          </div>

      </div>
      <div>
         <div class="antialiased bg-gray-100 text-gray-600 ">
   <div class="flex flex-col justify-center h-full">
      <div class="w-full mx-auto bg-white shadow-lg rounded-sm border border-gray-200">
         <header class="p-3 border-b border-gray-100">
            <h2 class="font-semibold text-gray-800">Contest records</h2>
         </header>
         <div class="py-3">
            <div class="overflow-x-auto">
               <table class="table-auto w-full">
                  <thead class="text-xs font-semibold uppercase text-gray-400 bg-gray-50">
                     <tr>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">No</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Name</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Mobile</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Email</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Postcode</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Question</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Answer</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Correct?</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Time (ms)</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-left">Receipt</div>
                        </th>
                        <th class="p-2 whitespace-nowrap">
                           <div class="font-semibold text-center">Submitted At</div>
                        </th>
                     </tr>
                  </thead>
                  <tbody class="text-sm divide-y divide-gray-100 text-xs">
                     <tr v-for="(rec,index) in records.data" :key="'rec-'+index">
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{ ((pageNumber-1)*pageSize)+ index+1}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="flex items-center">
                              <div class="font-medium text-gray-800">{{rec.fullname}}</div>
                           </div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.mobile}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.email}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.postcode}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.question}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.answer}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.correct?"Yes":"No"}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.timeTaken/1000}}</div>
                        </td>
                        <td class="p-2 whitespace-nowrap cursor-pointer">
                           <fa :icon="['fas','download']" @click="downloadFile(rec.receipt_url)"></fa>
                          
                        </td>
                        <td class="p-2 whitespace-nowrap">
                           <div class="text-left">{{rec.created_at | formatDate}}</div>
                        </td>
                     </tr>
                  </tbody>
               </table>
            </div>
         </div>
      </div>
   </div>
</div>


      </div>
    </div>
  </section>
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
        records: {},
        pageSize: 10,
        pageNumber: 1
    }
  },
  methods:{
      pageChanged(pageNum){
          this.pageNumber = pageNum;
        this.$axios.get(`${this.$config.restUrl}/api/contests?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}`)
        .then(response => {
            this.records = response.data;
        })
      },async logout(){
        await this.$auth.logout().then(response => {
            this.$router.push("/backstage/login");
        });
      },
      downloadFile(fileKey){
        console.log(`==${fileKey}`);
        this.$axios({
                    url: `${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`,
                    method: 'GET',
                    responseType: 'blob',
        })
        // this.$axios.get(`${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`)
        .then((response) => {
                     const fileURL = window.URL.createObjectURL(new Blob([response.data]));
                     const fileLink = document.createElement('a');
   
                     fileLink.href = fileURL;
                     fileLink.setAttribute('download', fileKey);
                     document.body.appendChild(fileLink);
   
                     fileLink.click();
                });
      },
       downloadExcel(){
        this.$axios({
                    url: `${this.$config.restUrl}/api/contests/excel`,
                    method: 'GET',
                    responseType: 'blob',
        })
        // this.$axios.get(`${this.$config.restUrl}/api/contests/receipt?fileKey=${fileKey}`)
        .then((response) => {
                     const fileURL = window.URL.createObjectURL(new Blob([response.data]));
                     const fileLink = document.createElement('a');
   
                     fileLink.href = fileURL;
                     fileLink.setAttribute('download', "summary.xlsx");
                     document.body.appendChild(fileLink);
   
                     fileLink.click();
                });
      }
  },
   created() {
       
        this.$axios.get(`${this.$config.restUrl}/api/contests?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}`)
        .then(response => {
            this.records = response.data;
        })
    }

}
</script>