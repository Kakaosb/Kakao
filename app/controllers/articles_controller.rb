class ArticlesController < ApplicationController
	  before_action :signed_in_user, only: [:create, :destroy]

  def index
    @articles = Article.all
    #@tag = Tag.all

    #@articles = Article.paginate(page: params[:page])
  end

   def show
    @article = Article.find(params[:id])
  end
  
	def new
    @article = Article.new
     #@tag = Tag.new
   
  end

def edit
  @article = Article.find(params[:id])
  end

  #def create
  #@article = Article.new(article_params)
 #  @article.save
 # redirect_to @article
#end
 
 def create
    @article = current_user.articles.build(article_params) #метод ассоциации belongs_to/has_many, возвращает новый объект Article (user_id = user.id)
    
      #.sub(/\w+mber/, 'july')
      #   /#(\w+)/  /#[^\s]+/

 


find_tags
 

flash[:success] = "Статья добавлена"



#rez = str.scan(/#[^\s]+/)
#flash[:success] =  str

  if @article.save
      redirect_to root_url
    else
      render 'welcome/index'
    end


     #@tag = Tag.new(tag_params)
  end


 def update
  @article = Article.find(params[:id])
 
  if @article.update(article_params)
    redirect_to @article
  else
    render 'edit'
  end
end

def destroy
  @article = Article.find(params[:id])
  @article.destroy
 
  redirect_to articles_path
end

private
  def article_params
    params.require(:article).permit(:title, :text, :user_id)
  end


   #def tag_params
   # params.require(:tag).permit(:tag)
  #end
end
